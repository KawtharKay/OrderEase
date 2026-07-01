using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetAllCustomerBalances
    {
        public record GetAllCustomerBalancesQuery() : IRequest<Result<ICollection<CustomerBalanceItem>>>;

        public class GetAllCustomerBalancesHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetAllCustomerBalancesQuery, Result<ICollection<CustomerBalanceItem>>>
        {
            public async Task<Result<ICollection<CustomerBalanceItem>>> Handle(GetAllCustomerBalancesQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var payments = await paymentRepository.GetAllAsync();

                    var grouped = payments
                        .Where(x => x.IsConfirmed)
                        .GroupBy(x => new { x.CustomerId, x.Customer.Name, x.Customer.Email })
                        .Select(g => new CustomerBalanceItem(
                            g.Key.CustomerId,
                            g.Key.Name,
                            g.Key.Email,
                            g.Sum(x => x.AmountTotal),
                            g.Sum(x => x.AmountPaid),
                            g.Sum(x => x.OutstandingBalance)))
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