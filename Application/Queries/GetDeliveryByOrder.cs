using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetDeliveryByOrder
    {
        public record GetDeliveryByOrderQuery(Guid OrderId) : IRequest<Result<GetDeliveryByOrderResponse>>;

        public class GetDeliveryByOrderValidator : AbstractValidator<GetDeliveryByOrderQuery>
        {
            public GetDeliveryByOrderValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order ID is required");
            }
        }

        public class GetDeliveryByOrderHandler(IDeliveryRepository deliveryRepository) : IRequestHandler<GetDeliveryByOrderQuery, Result<GetDeliveryByOrderResponse>>
        {
            public async Task<Result<GetDeliveryByOrderResponse>> Handle(GetDeliveryByOrderQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var delivery = await deliveryRepository.GetByOrderIdAsync(request.OrderId);
                    if (delivery is null) return Result<GetDeliveryByOrderResponse>.Failure("No delivery record found for this order");

                    return Result<GetDeliveryByOrderResponse>.Success(delivery.Adapt<GetDeliveryByOrderResponse>(), "Delivery details retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetDeliveryByOrderResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetDeliveryByOrderResponse(Guid Id, Guid OrderId, string DeliveryMethod);
    }
}