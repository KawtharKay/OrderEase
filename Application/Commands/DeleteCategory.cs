using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;
using static Application.Commands.UpdateCategory;

namespace Application.Commands
{
    public class DeleteCategory
    {
        public record DeleteCategoryCommand(Guid Id) : IRequest<Result<string>>;

        public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
        {
            public UpdateCategoryValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Category ID is required");
            }
        }

        public class DeleteCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var category = await categoryRepository.GetAsync(request.Id);
                    if (category == null) return Result<string>.Failure("Category not found");

                    category.IsDeleted = true;
                    await unitOfWork.SaveAsync();

                    return Result<string>.Success("Deleted!", "Category deleted successfully!");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occured: {ex.Message}");
                }
            }
        }
    }
}
