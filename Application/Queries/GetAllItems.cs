using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetAllItems
    {
        public record GetAllItemsQuery(string? Search, Guid? CategoryId, int Page = 1, int PageSize = 20)
            : IRequest<Result<PaginatedItemsResponse>>;

        public class GetAllItemsHandler(IItemRepository itemRepository) : IRequestHandler<GetAllItemsQuery, Result<PaginatedItemsResponse>>
        {
            public async Task<Result<PaginatedItemsResponse>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var page = request.Page < 1 ? 1 : request.Page;
                    var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

                    var (items, totalCount) = await itemRepository.SearchAsync(request.Search, request.CategoryId, page, pageSize);

                    var response = new PaginatedItemsResponse(
                        items.Adapt<ICollection<GetAllItemsResponse>>(),
                        totalCount,
                        page,
                        pageSize,
                        (int)Math.Ceiling(totalCount / (double)pageSize));

                    return Result<PaginatedItemsResponse>.Success(response, "Items retrieved successfully!");
                }
                catch (Exception ex)
                {
                    return Result<PaginatedItemsResponse>.Failure($"An error occured: {ex.Message}");
                }
            }
        }

        public record GetAllItemsResponse(Guid Id, Guid CategoryId, string Title, string ImageUrl, decimal Price, int Quantity, bool IsAvailable);

        public record PaginatedItemsResponse(ICollection<GetAllItemsResponse> Items, int TotalCount, int Page, int PageSize, int TotalPages);
    }
}
