using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Queries
{
    public class GetCustomerOrdersWithPayments
    {
        public record GetCustomerOrdersWithPaymentsQuery(Guid CustomerId) : IRequest<Result<ICollection<OrderWithPaymentItem>>>;

        public class GetCustomerOrdersWithPaymentsValidator : AbstractValidator<GetCustomerOrdersWithPaymentsQuery>
        {
            public GetCustomerOrdersWithPaymentsValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer ID is required");
            }
        }

        public class GetCustomerOrdersWithPaymentsHandler(IOrderRepository orderRepository, IPaymentRepository paymentRepository)
            : IRequestHandler<GetCustomerOrdersWithPaymentsQuery, Result<ICollection<OrderWithPaymentItem>>>
        {
            public async Task<Result<ICollection<OrderWithPaymentItem>>> Handle(GetCustomerOrdersWithPaymentsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var orders = await orderRepository.GetAllByCustomerIdAsync(request.CustomerId);
                    var payments = await paymentRepository.GetAllByCustomerIdAsync(request.CustomerId);

                    var result = orders.Select(order =>
                    {
                        var orderPayments = payments.Where(x => x.OrderId == order.Id && x.IsConfirmed).ToList();

                        var amountPaid = orderPayments.Sum(x => x.AmountPaid);
                        var outstanding = order.TotalPrice - amountPaid;

                        return new OrderWithPaymentItem(order.Id, order.OrderNumber, order.OrderStatus.ToString(), order.TotalPrice, amountPaid, outstanding, order.OrderDate);
                    })
                    .OrderByDescending(x => x.OrderDate)
                    .ToList();

                    return Result<ICollection<OrderWithPaymentItem>>.Success(result, "Customer orders with payment status retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<OrderWithPaymentItem>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record OrderWithPaymentItem(Guid OrderId, string OrderNumber, string OrderStatus, decimal OrderTotal, decimal AmountPaid, decimal OutstandingBalance, DateTime OrderDate);
    }
}