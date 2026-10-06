using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Queries
{
    public class GetStockMovementsByItem
    {
        public record GetStockMovementsByItemQuery(Guid ItemId) : IRequest<Result<ICollection<StockMovementDto>>>;

        public class GetStockMovementsByItemValidator : AbstractValidator<GetStockMovementsByItemQuery>
        {
            public GetStockMovementsByItemValidator()
            {
                RuleFor(x => x.ItemId)
                    .NotEmpty()
                    .WithMessage("Item is required");
            }
        }

        public class GetStockMovementsByItemHandler(IStockMovementRepository stockMovementRepository)
            : IRequestHandler<GetStockMovementsByItemQuery, Result<ICollection<StockMovementDto>>>
        {
            public async Task<Result<ICollection<StockMovementDto>>> Handle(GetStockMovementsByItemQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var movements = await stockMovementRepository.GetByItemIdAsync(request.ItemId);

                    var response = movements.Select(x => new StockMovementDto(x.QuantityAdded, x.CostPrice, x.Reference, x.DateReceived)).ToList();

                    return Result<ICollection<StockMovementDto>>.Success(response, "Stock history retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<StockMovementDto>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record StockMovementDto(int QuantityAdded, decimal CostPrice, string? Reference, DateTime DateReceived);
    }
}
