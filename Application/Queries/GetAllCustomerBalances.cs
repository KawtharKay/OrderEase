using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetAllCustomerBalances
    {
        public record GetAllCustomerBalancesQuery() : IRequest<Result<ICollection<CustomerBalanceItem>>>;

        public class GetAllCustomerBalancesHandler(IOrderRepository orderRepository, IPaymentRepository paymentRepository)
        : IRequestHandler<GetAllCustomerBalancesQuery, Result<ICollection<CustomerBalanceItem>>>
        {
            public async Task<Result<ICollection<CustomerBalanceItem>>> Handle(GetAllCustomerBalancesQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var orders = await orderRepository.GetAllAsync();
                    var payments = await paymentRepository.GetAllAsync();

                    var confirmedPaymentsByCustomer = payments
                        .Where(x => x.IsConfirmed)
                        .GroupBy(x => x.CustomerId)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.AmountPaid));

                    var grouped = orders
                        .GroupBy(o => new { o.CustomerId, o.Customer.Name, o.Customer.Email })
                        .Select(g =>
                        {
                            var totalBilled = g.Sum(o => o.TotalPrice);
                            var walletCovered = g.Sum(o => o.WalletAmountUsed);
                            var paystackPaid = confirmedPaymentsByCustomer.TryGetValue(g.Key.CustomerId, out var paid) ? paid : 0;
                            var totalPaid = walletCovered + paystackPaid;
                            var outstanding = Math.Max(0, totalBilled - totalPaid);

                            return new CustomerBalanceItem(g.Key.CustomerId, g.Key.Name, g.Key.Email, totalBilled, totalPaid, outstanding);
                        })
                        .ToList();

                    return Result<ICollection<CustomerBalanceItem>>.Success(grouped, "Customer balances retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<CustomerBalanceItem>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record CustomerBalanceItem(Guid CustomerId, string CustomerName, string CustomerEmail, decimal TotalBilled, decimal TotalPaid, decimal OutstandingBalance);
    }
}