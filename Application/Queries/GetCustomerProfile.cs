using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using MediatR;

namespace Application.Queries
{
    public class GetCustomerProfile
    {
        public record GetCustomerProfileQuery() : IRequest<Result<GetCustomerProfileResponse>>;

        public class GetCustomerProfileHandler(ICustomerRepository customerRepository, ICurrentUser currentUser)
            : IRequestHandler<GetCustomerProfileQuery, Result<GetCustomerProfileResponse>>
        {
            public async Task<Result<GetCustomerProfileResponse>> Handle(GetCustomerProfileQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var userId = currentUser.GetCurrentUserId();
                    var customer = await customerRepository.GetByUserIdAsync(userId);
                    if (customer is null) return Result<GetCustomerProfileResponse>.Failure("Customer profile not found");

                    return Result<GetCustomerProfileResponse>.Success(
                        new GetCustomerProfileResponse(customer.Id, customer.Name, customer.Email, customer.PhoneNumber, customer.Address),
                        "Customer profile retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetCustomerProfileResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetCustomerProfileResponse(Guid Id, string Name, string Email, string PhoneNumber, string Address);
    }
}