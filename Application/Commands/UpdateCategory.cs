using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Commands
{
    public class UpdateCategory
    {
        public record UpdateCategoryCommand(Guid Id, string Name ) : IRequest<Result<UpdateCategoryResponse>>;

        public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
        {
            public UpdateCategoryValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Category ID is required");

                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Category name is required")
                    .MaximumLength(100)
                    .WithMessage("Category name should not exceed 100 characters");
            }
        }

        public class UpdateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoryCommand, Result<UpdateCategoryResponse>>
        {
            public async Task<Result<UpdateCategoryResponse>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var category = await categoryRepository.GetAsync(request.Id);
                    if (category is null) return Result<UpdateCategoryResponse>.Failure("Category not found");

                    var nameExist = await categoryRepository.GetAsync(request.Name);
                    if (nameExist != null) return Result<UpdateCategoryResponse>.Failure("Category with this name already exist");

                    category.Name = request.Name;
                    categoryRepository.Update(category);
                    await unitOfWork.SaveAsync();

                    return Result<UpdateCategoryResponse>.Success(category.Adapt<UpdateCategoryResponse>(), "Category updated succesfully");
                }
                catch (Exception ex)
                {
                    return Result<UpdateCategoryResponse>.Failure($"An error occured: {ex.Message}");
                }
            }
        }

        public record UpdateCategoryResponse(Guid Id, string Name);
    }
}
