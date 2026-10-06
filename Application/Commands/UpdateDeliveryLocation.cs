using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands
{
    public class UpdateDeliveryLocation
    {
        public record UpdateDeliveryLocationCommand(Guid Id, string Name, decimal Fee, bool IsActive) : IRequest<Result<string>>;

        public class UpdateDeliveryLocationValidator : AbstractValidator<UpdateDeliveryLocationCommand>
        {
            public UpdateDeliveryLocationValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Location is required");

                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Location name is required")
                    .MaximumLength(150);

                RuleFor(x => x.Fee)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Fee cannot be negative");
            }
        }

        public class UpdateDeliveryLocationHandler(IDeliveryLocationRepository deliveryLocationRepository, IUnitOfWork unitOfWork)
            : IRequestHandler<UpdateDeliveryLocationCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateDeliveryLocationCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var location = await deliveryLocationRepository.GetAsync(request.Id);
                    if (location is null) return Result<string>.Failure("Delivery location not found");

                    location.Name = request.Name.Trim();
                    location.Fee = request.Fee;
                    location.IsActive = request.IsActive;

                    deliveryLocationRepository.Update(location);
                    await unitOfWork.SaveAsync();

                    return Result<string>.Success("Updated", "Delivery location updated");
                }
                catch (DbUpdateException)
                {
                    return Result<string>.Failure("A location with this name already exists");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}
