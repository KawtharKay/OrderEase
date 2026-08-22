using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Commands
{
    public class CreateDelivery
    {
        public record CreateDeliveryCommand(Guid OrderId, DeliveryMethod DeliveryMethod, string? DeliveryAddress, Guid? DeliveryLocationId)
            : IRequest<Result<CreateDeliveryResponse>>;

        public class CreateDeliveryValidator : AbstractValidator<CreateDeliveryCommand>
        {
            public CreateDeliveryValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order ID is required");

                RuleFor(x => x.DeliveryMethod)
                    .IsInEnum()
                    .WithMessage("Invalid delivery method");

                RuleFor(x => x.DeliveryAddress)
                    .NotEmpty()
                    .WithMessage("Delivery address is required for this delivery method")
                    .When(x => x.DeliveryMethod != DeliveryMethod.CustomerPickup);
            }
        }

        public class CreateDeliveryHandler(IDeliveryRepository deliveryRepository, IOrderRepository orderRepository,
            IDeliveryLocationRepository deliveryLocationRepository, IDeliveryChargeRepository deliveryChargeRepository,
            IUnitOfWork unitOfWork) : IRequestHandler<CreateDeliveryCommand, Result<CreateDeliveryResponse>>
        {
            public async Task<Result<CreateDeliveryResponse>> Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetAsync(request.OrderId);
                    if (order is null) return Result<CreateDeliveryResponse>.Failure("Order not found");

                    var existingDelivery = await deliveryRepository.GetByOrderIdAsync(request.OrderId);
                    if (existingDelivery != null) return Result<CreateDeliveryResponse>.Failure("A delivery record already exists for this order");

                    DeliveryLocation? matchedLocation = null;
                    if (request.DeliveryMethod == DeliveryMethod.DispatchRider && request.DeliveryLocationId.HasValue)
                    {
                        var candidate = await deliveryLocationRepository.GetAsync(request.DeliveryLocationId.Value);
                        if (candidate != null && candidate.IsActive) matchedLocation = candidate;
                    }

                    var delivery = new Delivery
                    {
                        OrderId = request.OrderId,
                        DeliveryMethod = request.DeliveryMethod,
                        DeliveryAddress = request.DeliveryMethod != DeliveryMethod.CustomerPickup ? request.DeliveryAddress : null,
                        DeliveryLocationId = matchedLocation?.Id,
                        DateCreated = DateTime.UtcNow
                    };

                    await deliveryRepository.AddAsync(delivery);

                    if (matchedLocation != null)
                    {
                        await deliveryChargeRepository.AddRangeAsync(new List<DeliveryCharge>
                        {
                            new()
                            {
                                OrderId = order.Id,
                                Label = $"Dispatch fee - {matchedLocation.Name}",
                                Amount = matchedLocation.Fee,
                                DateCreated = DateTime.UtcNow
                            }
                        });

                        order.TotalPrice = order.ItemsSubtotal + matchedLocation.Fee;
                        order.AmountOwed = order.TotalPrice; 
                        order.DeliveryFeeConfirmed = true;
                        orderRepository.Update(order);
                    }
                    else if (request.DeliveryMethod != DeliveryMethod.CustomerPickup)
                    {
                        order.DeliveryFeeConfirmed = false;
                        orderRepository.Update(order);
                    }

                    await unitOfWork.SaveAsync();

                    return Result<CreateDeliveryResponse>.Success(delivery.Adapt<CreateDeliveryResponse>(), "Delivery details saved successfully");
                }
                catch (Exception ex)
                {
                    return Result<CreateDeliveryResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record CreateDeliveryResponse(Guid Id, Guid OrderId, string DeliveryMethod, string? DeliveryAddress, Guid? DeliveryLocationId);
    }
}
