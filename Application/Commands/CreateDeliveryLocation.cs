using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands
{
    public class CreateDeliveryLocation
    {
        public record CreateDeliveryLocationCommand(string Name, decimal Fee) : IRequest<Result<CreateDeliveryLocationResponse>>;

        public class CreateDeliveryLocationValidator : AbstractValidator<CreateDeliveryLocationCommand>
        {
            public CreateDeliveryLocationValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Location name is required")
                    .MaximumLength(150);

                RuleFor(x => x.Fee)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Fee cannot be negative");
            }
        }

        public class CreateDeliveryLocationHandler(IDeliveryLocationRepository deliveryLocationRepository, IUnitOfWork unitOfWork)
            : IRequestHandler<CreateDeliveryLocationCommand, Result<CreateDeliveryLocationResponse>>
        {
            public async Task<Result<CreateDeliveryLocationResponse>> Handle(CreateDeliveryLocationCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var location = new DeliveryLocation
                    {
                        Name = request.Name.Trim(),
                        Fee = request.Fee,
                        IsActive = true,
                        DateCreated = DateTime.UtcNow
                    };

                    await deliveryLocationRepository.AddAsync(location);
                    await unitOfWork.SaveAsync();

                    return Result<CreateDeliveryLocationResponse>.Success(
                        new CreateDeliveryLocationResponse(location.Id, location.Name, location.Fee, location.IsActive),
                        "Delivery location added");
                }
                catch (DbUpdateException)
                {
                    return Result<CreateDeliveryLocationResponse>.Failure("A location with this name already exists");
                }
                catch (Exception ex)
                {
                    return Result<CreateDeliveryLocationResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record CreateDeliveryLocationResponse(Guid Id, string Name, decimal Fee, bool IsActive);
    }
}
