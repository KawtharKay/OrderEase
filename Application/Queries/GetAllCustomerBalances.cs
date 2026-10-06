using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllCustomerBalances
    {
        public record GetAllCustomerBalancesQuery() : IRequest<Result<ICollection<CustomerBalanceItem>>>;

        public class GetAllCustomerBalancesHandler(ICustomerRepository customerRepository)
            : IRequestHandler<GetAllCustomerBalancesQuery, Result<ICollection<CustomerBalanceItem>>>
        {
            public async Task<Result<ICollection<CustomerBalanceItem>>> Handle(
                GetAllCustomerBalancesQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var summaries = await customerRepository.GetAllWithBalanceSummaryAsync();

                    var result = summaries
                        .Select(x => new CustomerBalanceItem(
                            x.CustomerId,
                            x.CustomerName,
                            x.CustomerEmail,
                            x.TotalBilled,
                            x.TotalPaid,
                            x.OutstandingBalance))
                        .ToList();

                    return Result<ICollection<CustomerBalanceItem>>.Success(
                        result, "Customer balances retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<CustomerBalanceItem>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record CustomerBalanceItem(
            Guid CustomerId,
            string CustomerName,
            string CustomerEmail,
            decimal TotalBilled,
            decimal TotalPaid,
            decimal OutstandingBalance);
    }
}