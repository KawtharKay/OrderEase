using Application.Common.Dtos;
using Application.Repositories;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class UpdateDelivery
    {
        public record UpdateDeliveryCommand(Guid OrderId, DeliveryMethod DeliveryMethod) : IRequest<Result<string>>;

        public class UpdateDeliveryValidator : AbstractValidator<UpdateDeliveryCommand>
        {
            public UpdateDeliveryValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order ID is required");

                RuleFor(x => x.DeliveryMethod)
                    .IsInEnum()
                    .WithMessage("Invalid delivery method");
            }
        }

        public class UpdateDeliveryHandler(IDeliveryRepository deliveryRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateDeliveryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateDeliveryCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var delivery = await deliveryRepository.GetByOrderIdAsync(request.OrderId);
                    if (delivery is null) return Result<string>.Failure("No delivery record found for this order");

                    delivery.DeliveryMethod = request.DeliveryMethod;
                    deliveryRepository.Update(delivery);
                    await unitOfWork.SaveAsync();

                    return Result<string>.Success("Updated", "Delivery method updated successfully");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}