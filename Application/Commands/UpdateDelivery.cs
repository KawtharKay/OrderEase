using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class UpdateDelivery
    {
        public record UpdateDeliveryCommand(Guid OrderId, DeliveryMethod DeliveryMethod) : IRequest<Result<string>>;

        public class UpdateDeliveryValidator : AbstractValidator<UpdateDeliveryCommand>
        {
            public UpdateDeliveryValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order ID is required");

                RuleFor(x => x.DeliveryMethod)
                    .IsInEnum()
                    .WithMessage("Invalid delivery method");
            }
        }

        public class UpdateDeliveryHandler(IDeliveryRepository deliveryRepository, IOrderRepository orderRepository, IDeliveryChargeRepository deliveryChargeRepository,
            IPaymentRepository paymentRepository, IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository, IUnitOfWork unitOfWork)
            : IRequestHandler<UpdateDeliveryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateDeliveryCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var delivery = await deliveryRepository.GetByOrderIdAsync(request.OrderId);
                    if (delivery is null) return Result<string>.Failure("No delivery record found for this order");

                    var order = await orderRepository.GetAsync(request.OrderId);
                    if (order is null) return Result<string>.Failure("Order not found");

                    var previousMethod = delivery.DeliveryMethod;
                    delivery.DeliveryMethod = request.DeliveryMethod;
                    deliveryRepository.Update(delivery);

                    decimal refundAmount = 0;

                    if (request.DeliveryMethod == DeliveryMethod.CustomerPickup)
                    {
                        var existingCharges = await deliveryChargeRepository.GetByOrderIdAsync(request.OrderId);

                        if (existingCharges.Count > 0)
                        {
                            await deliveryChargeRepository.RemoveAllForOrderAsync(request.OrderId);

                            order.TotalPrice = order.ItemsSubtotal;

                            var confirmedPayments = await paymentRepository.GetByOrderIdAsync(order.Id);
                            var paystackPaid = confirmedPayments.Where(x => x.IsConfirmed).Sum(x => x.AmountPaid);
                            var alreadyPaid = order.WalletAmountUsed + paystackPaid;

                            refundAmount = Math.Max(0, alreadyPaid - order.TotalPrice);
                            order.AmountOwed = Math.Max(0, order.TotalPrice - alreadyPaid);

                            if (refundAmount > 0)
                            {
                                var wallet = await walletRepository.GetByCustomerAsync(order.CustomerId);
                                if (wallet == null)
                                {
                                    wallet = new Wallet { CustomerId = order.CustomerId, Balance = 0, DateCreated = DateTime.UtcNow };
                                    await walletRepository.AddAsync(wallet);
                                    await unitOfWork.SaveAsync();
                                }

                                wallet.Balance += refundAmount;
                                walletRepository.Update(wallet);

                                await walletTransactionRepository.AddAsync(new WalletTransaction
                                {
                                    WalletId = wallet.Id,
                                    Amount = refundAmount,
                                    Type = WalletTransactionType.Credit,
                                    Status = PaystackStatus.Successful,
                                    Description = $"Refund - delivery charges removed on order {order.OrderNumber} (switched to Customer Pickup)",
                                    DateCreated = DateTime.UtcNow
                                });
                            }
                        }

                        order.DeliveryFeeConfirmed = true;
                    }
                    else if (previousMethod != request.DeliveryMethod)
                    {
                        order.DeliveryFeeConfirmed = false;
                    }

                    orderRepository.Update(order);
                    await unitOfWork.SaveAsync();

                    return Result<string>.Success("Updated",
                        refundAmount > 0
                            ? $"Delivery method updated. ₦{refundAmount:N2} was refunded to the customer's wallet since Customer Pickup has no delivery charges."
                            : "Delivery method updated successfully");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}
