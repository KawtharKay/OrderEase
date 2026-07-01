using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;

namespace Application.Querries
{
    public class GetAllCategories
    {
        public record GetAllCategoriesQuery() : IRequest<Result<ICollection<GetAllCategoriesResponse>>>;
        public class GetAllCategoriesHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetAllCategoriesQuery, Result<ICollection<GetAllCategoriesResponse>>>
        {
            public async Task<Result<ICollection<GetAllCategoriesResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var categries = await categoryRepository.GetAllAsync();

                    return Result<ICollection<GetAllCategoriesResponse>>.Success(categries.Adapt<ICollection<GetAllCategoriesResponse>>(), "Categories retrieved successfully!");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetAllCategoriesResponse>>.Failure($"An error occured: {ex.Message}");
                }
            }
        }

        public record GetAllCategoriesResponse(Guid Id, string Name);
    }
}
