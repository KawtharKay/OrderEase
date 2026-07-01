using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Commands
{
    public class CreateOrder
    {

        public record CreateOrderCommand(Guid CustomerId, ICollection<OrderItemDto> Items) : IRequest<Result<CreateOrderResponse>>;

        public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
        {
            public CreateOrderValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer is required");

                RuleFor(x => x.Items)
                    .NotEmpty()
                    .WithMessage("Order must contain at least one item");

                RuleForEach(x => x.Items)
                    .ChildRules(item =>
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

        public class CreateOrderHandler(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, ICustomerRepository customerRepository, IItemRepository itemRepository,
          ISupplierRepository supplierRepository, IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository, INotificationService notificationService, IEmailService emailService, ILogger<CreateOrderHandler> logger, IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
        {
            public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetAsync(request.CustomerId);
                    if (customer is null) return Result<CreateOrderResponse>.Failure("Customer not found");

                    decimal totalPrice = 0;
                    var orderItemsToCreate = new List<OrderItem>();

                    foreach (var requestedItem in request.Items)
                    {
                        var item = await itemRepository.GetAsync(requestedItem.ItemId);
                        if (item == null) return Result<CreateOrderResponse>.Failure($"Item not found");

                        if (!item.IsAvailable) return Result<CreateOrderResponse>.Failure($"'{item.Title}' is currently unavailable");

                        if (item.Quantity < requestedItem.Quantity) return Result<CreateOrderResponse>.Failure($"Insufficient stock for '{item.Title}'. Only {item.Quantity} left");

                        var subTotal = item.Price * requestedItem.Quantity;
                        totalPrice += subTotal;

                        orderItemsToCreate.Add(new OrderItem
                        {
                            ItemId = item.Id,
                            Quantity = requestedItem.Quantity,
                            UnitPrice = item.Price,
                            SubTotal = subTotal,
                            DateCreated = DateTime.UtcNow
                        });

                        item.Quantity -= requestedItem.Quantity;
                        if (item.Quantity == 0)
                            item.IsAvailable = false;

                        itemRepository.Update(item);
                    }

                    var wallet = await walletRepository.GetByCustomerAsync(request.CustomerId);
                    decimal walletAmountUsed = 0;

                    if (wallet != null && wallet.Balance > 0)
                    {
                        walletAmountUsed = Math.Min(wallet.Balance, totalPrice);
                        wallet.Balance -= walletAmountUsed;
                        walletRepository.Update(wallet);
                    }

                    var amountOwed = totalPrice - walletAmountUsed;

                    var orderNumber = $"ORD/{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(0000, 9999)}";

                    var order = new Order
                    {
                        OrderNumber = orderNumber,
                        CustomerId = request.CustomerId,
                        OrderStatus = OrderStatus.Received,
                        TotalPrice = totalPrice,
                        OrderDate = DateTime.UtcNow,
                        DateCreated = DateTime.UtcNow
                    };

                    await orderRepository.AddAsync(order);
                    await unitOfWork.SaveAsync();

                    foreach (var orderItem in orderItemsToCreate)
                    {
                        orderItem.OrderId = order.Id;
                    }

                    await orderItemRepository.AddRangeAsync(orderItemsToCreate);

                    if (walletAmountUsed > 0)
                    {
                        var walletTransaction = new WalletTransaction
                        {
                            WalletId = wallet!.Id,
                            OrderId = order.Id,
                            Amount = walletAmountUsed,
                            Type = WalletTransactionType.Debit,
                            Status = PaystackStatus.Successful,
                            Description = $"Applied to order {order.OrderNumber}",
                            DateCreated = DateTime.UtcNow
                        };
                        await walletTransactionRepository.AddAsync(walletTransaction);
                    }

                    await unitOfWork.SaveAsync();

                    var supplier = await supplierRepository.GetFirstAsync();
                    if (supplier != null)
                    {
                        await notificationService.SendNotificationAsync(supplier.UserId, "New Order Received", $"{customer.Name} placed a new order: " +
                            $"{order.OrderNumber} for ₦{order.TotalPrice:N2}", "NewOrder", order.Id);

                        try
                        {
                            await emailService.SendGenericEmailAsync(
                                supplier.Email,
                                "New Order on OrderEase",
                                $"<h2>New Order Received</h2><p>{customer.Name} just placed an order: {order.OrderNumber}.</p>");
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to send new order email notification to supplier {SupplierEmail}", supplier.Email);
                        }
                    }

                    var message = walletAmountUsed > 0
                        ? $"Order placed. ₦{walletAmountUsed:N2} was deducted from your wallet. Remaining balance to pay: ₦{amountOwed:N2}"
                        : "Order placed successfully";

                    return Result<CreateOrderResponse>.Success(new CreateOrderResponse(order.Id, order.OrderNumber, order.TotalPrice), "Order placed successfully");
                }

                catch (DbUpdateConcurrencyException)
                {
                    return Result<CreateOrderResponse>.Failure(
                        "This item just got out of stock. Please try again.");
                }

                catch (Exception ex)
                {
                    return Result<CreateOrderResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record OrderItemDto(Guid ItemId, int Quantity);
        public record CreateOrderResponse(Guid Id, string OrderNumber, decimal TotalPrice);
    }
}