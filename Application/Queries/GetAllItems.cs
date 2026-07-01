using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetAllItems
    {
        public record GetAllItemsQuery() : IRequest<Result<ICollection<GetAllItemsResponse>>>;
        public class GetAllItemsHandler(IItemRepository itemRepository) : IRequestHandler<GetAllItemsQuery, Result<ICollection<GetAllItemsResponse>>>
        {
            public async Task<Result<ICollection<GetAllItemsResponse>>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var items = await itemRepository.GetAllAsync();

                    return Result<ICollection<GetAllItemsResponse>>.Success(items.Adapt<ICollection<GetAllItemsResponse>>(), "Items retrieved successfully!");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetAllItemsResponse>>.Failure($"An error occured: {ex.Message}");
                }
            }
        }

        public record GetAllItemsResponse(Guid Id, Guid CategoryId, string Title, string ImageUrl, decimal Price, int Quantity);
    }
}
