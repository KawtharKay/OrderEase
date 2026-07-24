using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using MediatR;

namespace Application.Queries
{
    public class GetSupplierProfile
    {
        public record GetSupplierProfileQuery() : IRequest<Result<GetSupplierProfileResponse>>;

        public class GetSupplierProfileHandler(ISupplierRepository supplierRepository, ICurrentUser currentUser)
            : IRequestHandler<GetSupplierProfileQuery, Result<GetSupplierProfileResponse>>
        {
            public async Task<Result<GetSupplierProfileResponse>> Handle(GetSupplierProfileQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var userId = currentUser.GetCurrentUserId();
                    var supplier = await supplierRepository.GetByUserIdAsync(userId);
                    if (supplier is null) return Result<GetSupplierProfileResponse>.Failure("Supplier profile not found");

                    return Result<GetSupplierProfileResponse>.Success(
                        new GetSupplierProfileResponse(supplier.Id, supplier.Name, supplier.Email, supplier.PhoneNumber, supplier.Address),
                        "Supplier profile retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetSupplierProfileResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetSupplierProfileResponse(Guid Id, string Name, string Email, string PhoneNumber, string Address);
    }
}