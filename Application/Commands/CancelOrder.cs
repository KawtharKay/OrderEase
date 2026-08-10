using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class CancelOrder
    {
        public record CancelOrderCommand(Guid OrderId, Guid CustomerId) : IRequest<Result<string>>;

        public class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
        {
            public CancelOrderValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order ID is required");
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer ID is required");
            }
        }

        public class CancelOrderHandler(IOrderRepository orderRepository, IItemRepository itemRepository, IPaymentRepository paymentRepository,
            IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository, ISupplierRepository supplierRepository,
            INotificationService notificationService, IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetAsync(request.OrderId);
                    if (order == null) return Result<string>.Failure("Order not found");

                    if (order.CustomerId != request.CustomerId) return Result<string>.Failure("This order does not belong to this customer");

                    if (order.OrderStatus == OrderStatus.Delivered) return Result<string>.Failure("This order has already been delivered and can't be cancelled");

                    if (order.OrderStatus == OrderStatus.Cancelled) return Result<string>.Failure("This order is already cancelled");

                    foreach (var orderItem in order.OrderItems)
                    {
                        var item = await itemRepository.GetAsync(orderItem.ItemId);
                        if (item != null)
                        {
                            item.Quantity += orderItem.Quantity;
                            if (item.Quantity > 0 && !item.IsAvailable) item.IsAvailable = true;
                            itemRepository.Update(item);
                        }
                    }

                    var confirmedPayments = await paymentRepository.GetByOrderIdAsync(order.Id);
                    var paystackPaid = confirmedPayments.Where(x => x.IsConfirmed).Sum(x => x.AmountPaid);
                    var totalRefund = order.WalletAmountUsed + paystackPaid;

                    if (totalRefund > 0)
                    {
                        var wallet = await walletRepository.GetByCustomerAsync(request.CustomerId);
                        if (wallet == null)
                        {
                            wallet = new Wallet
                            {
                                CustomerId = request.CustomerId,
                                Balance = 0,
                                DateCreated = DateTime.UtcNow
                            };
                            await walletRepository.AddAsync(wallet);
                            await unitOfWork.SaveAsync();
                        }

                        wallet.Balance += totalRefund;
                        walletRepository.Update(wallet);

                        await walletTransactionRepository.AddAsync(new WalletTransaction
                        {
                            WalletId = wallet.Id,
                            Amount = totalRefund,
                            Type = WalletTransactionType.Credit,
                            Status = PaystackStatus.Successful,
                            Description = $"Refund for cancelled order {order.OrderNumber}",
                            DateCreated = DateTime.UtcNow
                        });
                    }

                    order.OrderStatus = OrderStatus.Cancelled;
                    order.AmountOwed = 0;
                    orderRepository.Update(order);
                    await unitOfWork.SaveAsync();

                    var supplier = await supplierRepository.GetFirstAsync();
                    if (supplier != null)
                    {
                        await notificationService.SendNotificationAsync(supplier.UserId, "Order Cancelled",
                            $"{order.Customer.Name} cancelled order {order.OrderNumber}.", "OrderStatusChanged", order.Id);
                    }

                    return Result<string>.Success("Cancelled",
                        totalRefund > 0
                            ? $"Order cancelled. ₦{totalRefund:N2} has been refunded to your wallet."
                            : "Order cancelled successfully");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}