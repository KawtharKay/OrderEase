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
        public record CreateDeliveryCommand(Guid OrderId, DeliveryMethod DeliveryMethod) : IRequest<Result<CreateDeliveryResponse>>;

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
            }
        }

        public class CreateDeliveryHandler(IDeliveryRepository deliveryRepository, IOrderRepository orderRepository,
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

                    var delivery = new Delivery
                    {
                        OrderId = request.OrderId,
                        DeliveryMethod = request.DeliveryMethod,
                        DateCreated = DateTime.UtcNow
                    };

                    await deliveryRepository.AddAsync(delivery);
                    await unitOfWork.SaveAsync();

                    return Result<CreateDeliveryResponse>.Success(delivery.Adapt<CreateDeliveryResponse>(), "Delivery details saved successfully");
                }
                catch (Exception ex)
                {
                    return Result<CreateDeliveryResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record CreateDeliveryResponse(Guid Id, Guid OrderId, string DeliveryMethod);
    }
}