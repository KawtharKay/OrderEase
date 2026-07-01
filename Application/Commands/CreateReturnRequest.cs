using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands
{
    public class CreateReturnRequest
    {
        public record ReturnItemDto(Guid ItemId, int Quantity);

        public record CreateReturnRequestCommand(Guid CustomerId, Guid OrderId, Guid CategoryId, string Reason, ICollection<ReturnItemDto> Items) 
            : IRequest<Result<CreateReturnRequestResponse>>;

        public class CreateReturnRequestValidator : AbstractValidator<CreateReturnRequestCommand>
        {
            public CreateReturnRequestValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer is required");

                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order is required");

                RuleFor(x => x.CategoryId)
                    .NotEmpty()
                    .WithMessage("Category is required");

                RuleFor(x => x.Reason)
                    .NotEmpty()
                    .WithMessage("Reason is required")
                    .MaximumLength(1000)
                    .WithMessage("Reason cannot exceed 1000 characters");

                RuleFor(x => x.Items)
                    .NotEmpty()
                    .WithMessage("At least one item must be selected for return");

                RuleForEach(x => x.Items).ChildRules(item =>
                {
                    item.RuleFor(x => x.ItemId)
                        .NotEmpty()
                        .WithMessage("Item ID is required");

                    item.RuleFor(x => x.Quantity)
                        .GreaterThan(0)
                        .WithMessage("Quantity must be greater than zero");
                });
            }
        }

        public class CreateReturnRequestHandler(IReturnRequestRepository returnRequestRepository, IReturnRequestItemRepository returnRequestItemRepository,
            ICustomerRepository customerRepository, IOrderRepository orderRepository, ICategoryRepository categoryRepository, IItemRepository itemRepository,
            ISupplierRepository supplierRepository, INotificationService notificationService, IEmailService emailService, IUnitOfWork unitOfWork, ILogger<CreateReturnRequestHandler> logger)
            : IRequestHandler<CreateReturnRequestCommand, Result<CreateReturnRequestResponse>>
        {
            public async Task<Result<CreateReturnRequestResponse>> Handle(CreateReturnRequestCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetAsync(request.CustomerId);
                    if (customer is null) return Result<CreateReturnRequestResponse>.Failure("Customer not found");

                    var order = await orderRepository.GetAsync(request.OrderId);
                    if (order is null) return Result<CreateReturnRequestResponse>.Failure("Order not found");

                    if (order.CustomerId != request.CustomerId) return Result<CreateReturnRequestResponse>.Failure("This order does not belong to this customer");

                    var category = await categoryRepository.GetAsync(request.CategoryId);
                    if (category is null) return Result<CreateReturnRequestResponse>.Failure("Category not found");

                    var returnItemsToCreate = new List<ReturnRequestItem>();

                    foreach (var requestedItem in request.Items)
                    {
                        var orderedItem = order.OrderItems.FirstOrDefault(x => x.ItemId == requestedItem.ItemId);
                        if (orderedItem is null) return Result<CreateReturnRequestResponse>.Failure("One of the items was not part of this order");

                        if (requestedItem.Quantity > orderedItem.Quantity) return Result<CreateReturnRequestResponse>.Failure($"Cannot return more than the {orderedItem.Quantity} ordered for this item");

                        returnItemsToCreate.Add(new ReturnRequestItem
                        {
                            ItemId = requestedItem.ItemId,
                            Quantity = requestedItem.Quantity,
                            DateCreated = DateTime.UtcNow
                        });
                    }

                    var returnRequest = new ReturnRequest
                    {
                        CustomerId = request.CustomerId,
                        OrderId = request.OrderId,
                        CategoryId = request.CategoryId,
                        Reason = request.Reason,
                        Status = ReturnRequestStatus.Pending,
                        DateCreated = DateTime.UtcNow
                    };

                    await returnRequestRepository.AddAsync(returnRequest);
                    await unitOfWork.SaveAsync();

                    foreach (var item in returnItemsToCreate)
                    {
                        item.ReturnRequestId = returnRequest.Id;
                    }

                    await returnRequestItemRepository.AddRangeAsync(returnItemsToCreate);
                    await unitOfWork.SaveAsync();

                    var supplier = await supplierRepository.GetFirstAsync();
                    if (supplier != null)
                    {
                        await notificationService.SendNotificationAsync(supplier.UserId, "New Return Request", $"{customer.Name} submitted a return request for" +
                            $" order {order.OrderNumber}", "ReturnRequested", returnRequest.Id);

                        try
                        {
                            await emailService.SendGenericEmailAsync(supplier.Email, "New Return Request on OrderEase",$"<h2>Return Request Submitted</h2><p>{customer.Name} " +
                                $"has requested a return for order {order.OrderNumber}.</p><p>Reason: {request.Reason}</p>");
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to send return request email to supplier");
                        }
                    }

                    return Result<CreateReturnRequestResponse>.Success(new CreateReturnRequestResponse(returnRequest.Id), "Return request submitted successfully");
                }
                catch (Exception ex)
                {
                    return Result<CreateReturnRequestResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record CreateReturnRequestResponse(Guid Id);
    }
}