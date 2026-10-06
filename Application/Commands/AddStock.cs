using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class AddStock
    {
        public record AddStockCommand(Guid ItemId, int QuantityAdded, decimal CostPrice, string? Reference) : IRequest<Result<AddStockResponse>>;

        public class AddStockValidator : AbstractValidator<AddStockCommand>
        {
            public AddStockValidator()
            {
                RuleFor(x => x.ItemId)
                    .NotEmpty()
                    .WithMessage("Item is required");

                RuleFor(x => x.QuantityAdded)
                    .GreaterThan(0)
                    .WithMessage("Quantity received must be greater than zero");

                RuleFor(x => x.CostPrice)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Cost price cannot be negative");

                RuleFor(x => x.Reference)
                    .MaximumLength(200)
                    .WithMessage("Reference cannot exceed 200 characters");
            }
        }

        public class AddStockHandler(IItemRepository itemRepository, IStockMovementRepository stockMovementRepository, IUnitOfWork unitOfWork)
            : IRequestHandler<AddStockCommand, Result<AddStockResponse>>
        {
            public async Task<Result<AddStockResponse>> Handle(AddStockCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var item = await itemRepository.GetAsync(request.ItemId);
                    if (item is null) return Result<AddStockResponse>.Failure("Item not found");

                    var previousQuantity = item.Quantity;

                    item.Quantity += request.QuantityAdded;
                    item.CostPrice = request.CostPrice;
                    if (item.Quantity > 0 && !item.IsAvailable) item.IsAvailable = true;
                    itemRepository.Update(item);

                    await stockMovementRepository.AddAsync(new StockMovement
                    {
                        ItemId = item.Id,
                        QuantityAdded = request.QuantityAdded,
                        CostPrice = request.CostPrice,
                        Reference = request.Reference,
                        DateReceived = DateTime.UtcNow,
                        DateCreated = DateTime.UtcNow
                    });

                    await unitOfWork.SaveAsync();

                    return Result<AddStockResponse>.Success(new AddStockResponse(item.Id, previousQuantity, request.QuantityAdded, item.Quantity),
                        $"Added {request.QuantityAdded} units. Stock is now {item.Quantity}.");
                }
                catch (Exception ex)
                {
                    return Result<AddStockResponse>.Failure($"An error occured: {ex.Message}");
                }
            }
        }

        public record AddStockResponse(Guid ItemId, int PreviousQuantity, int QuantityAdded, int NewQuantity);
    }
}
