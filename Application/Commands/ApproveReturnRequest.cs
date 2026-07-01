using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Commands
{
    public class ApproveReturnRequest
    {
        public record ApproveReturnRequestCommand(Guid ReturnRequestId) : IRequest<Result<string>>;

        public class ApproveReturnRequestValidator : AbstractValidator<ApproveReturnRequestCommand>
        {
            public ApproveReturnRequestValidator()
            {
                RuleFor(x => x.ReturnRequestId)
                    .NotEmpty()
                    .WithMessage("Return request ID is required");
            }
        }

        public class ApproveReturnRequestHandler(IReturnRequestRepository returnRequestRepository, IItemRepository itemRepository, INotificationService notificationService, IUnitOfWork unitOfWork, ILogger<ApproveReturnRequestHandler> logger)
            : IRequestHandler<ApproveReturnRequestCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(ApproveReturnRequestCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var returnRequest = await returnRequestRepository.GetAsync(request.ReturnRequestId);
                    if (returnRequest is null) return Result<string>.Failure("Return request not found");

                    if (returnRequest.Status != ReturnRequestStatus.Pending) return Result<string>.Failure("This return request has already been processed");

                    foreach (var returnItem in returnRequest.ReturnRequestItems)
                    {
                        var item = await itemRepository.GetAsync(returnItem.ItemId);
                        if (item != null)
                        {
                            item.Quantity += returnItem.Quantity;

                            if (item.Quantity > 0 && !item.IsAvailable)
                                item.IsAvailable = true;

                            itemRepository.Update(item);
                        }
                    }

                    returnRequest.Status = ReturnRequestStatus.Approved;
                    returnRequestRepository.Update(returnRequest);
                    await unitOfWork.SaveAsync();

                    await notificationService.SendNotificationAsync(returnRequest.Customer.UserId, "Return Request Approved", $"Your return request for order {returnRequest.Order.OrderNumber} has been approved", "ReturnApproved", returnRequest.Id);

                    return Result<string>.Success("Approved", "Return request approved and stock updated");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Result<string>.Failure("Item stock was just updated elsewhere. Please try again.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error approving return request {ReturnRequestId}", request.ReturnRequestId);
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}