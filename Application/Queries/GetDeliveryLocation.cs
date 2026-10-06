using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetDeliveryLocations
    {
        public record GetDeliveryLocationsQuery(bool ActiveOnly) : IRequest<Result<ICollection<GetDeliveryLocationsResponse>>>;

        public class GetDeliveryLocationsHandler(IDeliveryLocationRepository deliveryLocationRepository)
            : IRequestHandler<GetDeliveryLocationsQuery, Result<ICollection<GetDeliveryLocationsResponse>>>
        {
            public async Task<Result<ICollection<GetDeliveryLocationsResponse>>> Handle(GetDeliveryLocationsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var locations = await deliveryLocationRepository.GetAllAsync(request.ActiveOnly);
                    return Result<ICollection<GetDeliveryLocationsResponse>>.Success(
                        locations.Adapt<ICollection<GetDeliveryLocationsResponse>>(), "Delivery locations retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetDeliveryLocationsResponse>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetDeliveryLocationsResponse(Guid Id, string Name, decimal Fee, bool IsActive);
    }
}
