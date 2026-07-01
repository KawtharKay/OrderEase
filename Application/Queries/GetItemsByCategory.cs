using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;
using static Application.Queries.GetItemsByCategory.GetItemsByCategoryHandler;

namespace Application.Queries
{
    public class GetItemsByCategory
    {
        public record GetItemsByCategoryQuery(Guid CategoryId) : IRequest<Result<ICollection<GetItemsByCategoryResponse>>>;
        public class GetItemsByCategoryHandler(ICategoryRepository categoryRepository, IItemRepository itemRepository) : IRequestHandler<GetItemsByCategoryQuery, Result<ICollection<GetItemsByCategoryResponse>>>
        {
            public async Task<Result<ICollection<GetItemsByCategoryResponse>>> Handle(GetItemsByCategoryQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var category = await categoryRepository.GetAsync(request.CategoryId);
                    if (category is null) return Result<ICollection<GetItemsByCategoryResponse>>.Failure("Category not found");

                    var items = await itemRepository.GetByCategoryIdAsync(request.CategoryId);
                    return Result<ICollection<GetItemsByCategoryResponse>>.Success(items.Adapt<ICollection<GetItemsByCategoryResponse>>(), "Items retrieved successfully!");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetItemsByCategoryResponse>>.Failure($"An error occured: {ex.Message}");
                }
            }

        }
        public record GetItemsByCategoryResponse(Guid Id, string Title, string ImageUrl, decimal Price, int Quantity);
    }
}
