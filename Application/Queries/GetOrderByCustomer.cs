using Application.Common.Dtos;
using Application.Repositories;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Queries
{
    public class GetOrdersByCustomer
    {
        public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<Result<ICollection<GetOrdersByCustomerResponse>>>;

        public class GetOrdersByCustomerValidator : AbstractValidator<GetOrdersByCustomerQuery>
        {
            public GetOrdersByCustomerValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer ID is required");
            }
        }

        public class GetOrdersByCustomerHandler(IOrderRepository orderRepository, IPaymentRepository paymentRepository)
            : IRequestHandler<GetOrdersByCustomerQuery, Result<ICollection<GetOrdersByCustomerResponse>>>
        {
            public async Task<Result<ICollection<GetOrdersByCustomerResponse>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var orders = await orderRepository.GetAllByCustomerIdAsync(request.CustomerId);

                    // One query for every payment this customer has ever made, grouped by order
                    // in memory - avoids firing a separate payment lookup per order in the list.
                    var allPayments = await paymentRepository.GetAllByCustomerIdAsync(request.CustomerId);
                    var confirmedByOrder = allPayments
                        .Where(p => p.IsConfirmed)
                        .GroupBy(p => p.OrderId)
                        .ToDictionary(g => g.Key, g => g.Sum(p => p.AmountPaid));

                    var response = orders.Select(o =>
                    {
                        var paystackPaid = confirmedByOrder.TryGetValue(o.Id, out var paid) ? paid : 0;
                        var totalPaid = o.WalletAmountUsed + paystackPaid;
                        var outstanding = o.OrderStatus == OrderStatus.Cancelled ? 0 : Math.Max(0, o.TotalPrice - totalPaid);

                        return new GetOrdersByCustomerResponse(o.Id, o.OrderNumber, o.OrderStatus.ToString(), o.TotalPrice, outstanding, o.DeliveryFeeConfirmed, o.OrderDate);
                    })
                    .OrderByDescending(x => x.OrderDate)
                    .ToList();

                    return Result<ICollection<GetOrdersByCustomerResponse>>.Success(response, "Orders retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetOrdersByCustomerResponse>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetOrdersByCustomerResponse(Guid Id, string OrderNumber, string OrderStatus, decimal TotalPrice,
            decimal OutstandingBalance, bool DeliveryFeeConfirmed, DateTime OrderDate);
    }
}
