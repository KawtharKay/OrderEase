using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class SetDeliveryCharges
    {
        public record ChargeLineDto(string Label, decimal Amount);

        public record SetDeliveryChargesCommand(Guid OrderId, ICollection<ChargeLineDto> Charges) : IRequest<Result<SetDeliveryChargesResponse>>;

        public class SetDeliveryChargesValidator : AbstractValidator<SetDeliveryChargesCommand>
        {
            public SetDeliveryChargesValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order is required");

                RuleForEach(x => x.Charges).ChildRules(charge =>
                {
                    charge.RuleFor(x => x.Label)
                        .NotEmpty()
                        .WithMessage("Each charge needs a label")
                        .MaximumLength(100);

                    charge.RuleFor(x => x.Amount)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Charge amount cannot be negative");
                });
            }
        }

        public class SetDeliveryChargesHandler(IOrderRepository orderRepository, IDeliveryRepository deliveryRepository, IDeliveryChargeRepository deliveryChargeRepository,
            IPaymentRepository paymentRepository, INotificationService notificationService, IUnitOfWork unitOfWork) : IRequestHandler<SetDeliveryChargesCommand, Result<SetDeliveryChargesResponse>>
        {
            public async Task<Result<SetDeliveryChargesResponse>> Handle(SetDeliveryChargesCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetAsync(request.OrderId);
                    if (order is null) return Result<SetDeliveryChargesResponse>.Failure("Order not found");

                    if (order.OrderStatus != OrderStatus.Received && order.OrderStatus != OrderStatus.Processing)
                        return Result<SetDeliveryChargesResponse>.Failure(
                            $"Delivery charges can only be set while the order is Received or Processing (currently {order.OrderStatus})");

                    var delivery = await deliveryRepository.GetByOrderIdAsync(order.Id);
                    if (delivery != null && delivery.DeliveryMethod == DeliveryMethod.CustomerPickup && request.Charges.Count > 0)
                        return Result<SetDeliveryChargesResponse>.Failure("This order is Customer Pickup - there's no delivery leg to charge a fee for");

                    await deliveryChargeRepository.RemoveAllForOrderAsync(order.Id);

                    var newCharges = request.Charges.Select(c => new DeliveryCharge
                    {
                        OrderId = order.Id,
                        Label = c.Label,
                        Amount = c.Amount,
                        DateCreated = DateTime.UtcNow
                    }).ToList();

                    if (newCharges.Count > 0)
                        await deliveryChargeRepository.AddRangeAsync(newCharges);

                    var deliveryFeeTotal = newCharges.Sum(c => c.Amount);
                    order.TotalPrice = order.ItemsSubtotal + deliveryFeeTotal;

                    var confirmedPayments = await paymentRepository.GetByOrderIdAsync(order.Id);
                    var paystackPaid = confirmedPayments.Where(x => x.IsConfirmed).Sum(x => x.AmountPaid);
                    var alreadyPaid = order.WalletAmountUsed + paystackPaid;
                    order.AmountOwed = Math.Max(0, order.TotalPrice - alreadyPaid);

                    var wasConfirmed = order.DeliveryFeeConfirmed;
                    order.DeliveryFeeConfirmed = true;

                    orderRepository.Update(order);
                    await unitOfWork.SaveAsync();

                    if (!wasConfirmed)
                    {
                        await notificationService.SendNotificationAsync(order.Customer.UserId, "Delivery fee confirmed",
                            $"Your order {order.OrderNumber} total is now ₦{order.TotalPrice:N2}. You can now complete payment.",
                            "OrderStatusChanged", order.Id);
                    }

                    return Result<SetDeliveryChargesResponse>.Success(
                        new SetDeliveryChargesResponse(order.Id, order.ItemsSubtotal, deliveryFeeTotal, order.TotalPrice, order.AmountOwed),
                        "Delivery charges updated.");
                }
                catch (Exception ex)
                {
                    return Result<SetDeliveryChargesResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record SetDeliveryChargesResponse(Guid OrderId, decimal ItemsSubtotal, decimal DeliveryFeeTotal, decimal TotalPrice, decimal AmountOwed);
    }
}
