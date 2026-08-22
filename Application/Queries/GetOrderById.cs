using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Queries
{
    public class GetOrderById
    {
        public record GetOrderByIdQuery(Guid Id) : IRequest<Result<GetOrderByIdResponse>>;

        public class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>
        {
            public GetOrderByIdValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Order ID is required");
            }
        }

        public class GetOrderByIdHandler(IOrderRepository orderRepository, IPaymentRepository paymentRepository) : IRequestHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
        {
            public async Task<Result<GetOrderByIdResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetAsync(request.Id);
                    if (order == null) return Result<GetOrderByIdResponse>.Failure("Order not found");

                    var confirmedPayments = await paymentRepository.GetByOrderIdAsync(order.Id);
                    var paystackPaid = confirmedPayments.Where(x => x.IsConfirmed).Sum(x => x.AmountPaid);
                    var totalPaid = order.WalletAmountUsed + paystackPaid;
                    var outstanding = Math.Max(0, order.TotalPrice - totalPaid);

                    var response = new GetOrderByIdResponse(order.Id, order.OrderNumber, order.CustomerId, order.OrderStatus.ToString(),
                    order.ItemsSubtotal, order.DeliveryCharges.Sum(x => x.Amount), order.TotalPrice, order.DeliveryFeeConfirmed,
                    order.WalletAmountUsed, totalPaid, outstanding, order.OrderDate,
                    order.OrderItems.Select(x => new OrderItemDto(
                    x.ItemId,
                    x.Item.CategoryId,
                    x.Item.Title,
                    x.Quantity,
                    x.UnitPrice,
                    x.SubTotal)).ToList(),
                    order.DeliveryCharges.Select(x => new DeliveryChargeDto(x.Label, x.Amount)).ToList(),
                    order.StatusHistory
                        .OrderBy(x => x.ChangedAt)
                        .Select(x => new OrderStatusHistoryDto(
                            x.PreviousStatus?.ToString(),
                            x.NewStatus.ToString(),
                            x.ChangedAt)).ToList());

                    return Result<GetOrderByIdResponse>.Success(response, "Order retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetOrderByIdResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record OrderItemDto(Guid ItemId, Guid CategoryId, string Title, int Quantity, decimal UnitPrice, decimal SubTotal);

        public record DeliveryChargeDto(string Label, decimal Amount);

        public record OrderStatusHistoryDto(string? PreviousStatus, string NewStatus, DateTime ChangedAt);

        public record GetOrderByIdResponse(Guid Id, string OrderNumber, Guid CustomerId, string OrderStatus,
            decimal ItemsSubtotal, decimal DeliveryFeeTotal, decimal TotalPrice, bool DeliveryFeeConfirmed,
            decimal WalletAmountUsed, decimal TotalPaid, decimal OutstandingBalance, DateTime OrderDate, ICollection<OrderItemDto> OrderItems,
            ICollection<DeliveryChargeDto> DeliveryCharges, ICollection<OrderStatusHistoryDto> StatusHistory);
    }
}
