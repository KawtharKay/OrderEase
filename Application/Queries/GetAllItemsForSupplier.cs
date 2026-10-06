using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetAllItemsForSupplier
    {
        public record GetAllItemsForSupplierQuery(string? Search, Guid? CategoryId, int Page = 1, int PageSize = 20)
            : IRequest<Result<PaginatedItemsResponse>>;

        public class GetAllItemsForSupplierHandler(IItemRepository itemRepository) : IRequestHandler<GetAllItemsForSupplierQuery, Result<PaginatedItemsResponse>>
        {
            public async Task<Result<PaginatedItemsResponse>> Handle(GetAllItemsForSupplierQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var page = request.Page < 1 ? 1 : request.Page;
                    var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

                    var (items, totalCount) = await itemRepository.SearchAsync(request.Search, request.CategoryId, page, pageSize);

                    var response = new PaginatedItemsResponse(
                        items.Adapt<ICollection<GetAllItemsForSupplierResponse>>(),
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

        public record GetAllItemsForSupplierResponse(Guid Id, Guid CategoryId, string Title, string ImageUrl, decimal Price, decimal CostPrice, int Quantity, bool IsAvailable);

        public record PaginatedItemsResponse(ICollection<GetAllItemsForSupplierResponse> Items, int TotalCount, int Page, int PageSize, int TotalPages);
    }
}
