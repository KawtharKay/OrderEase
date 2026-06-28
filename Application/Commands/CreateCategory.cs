using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class CreateCategory
    {
        public record CreateCategoryCommand(string Name) : IRequest<Result<CreateCategoryResponse>>;

        public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
        {
            public CreateCategoryValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Category name is required")
                    .MaximumLength(100)
                    .WithMessage("Category name should not exceed 100 characters");
            }
        }

        public class CreateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, Result<CreateCategoryResponse>>
        {
            public async Task<Result<CreateCategoryResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var categoryExists = await categoryRepository.GetAsync(request.Name);
                    if (categoryExists != null) return Result<CreateCategoryResponse>.Failure("Category already exist!");

                    var category = new Category
                    {
                        Name = request.Name,
                        DateCreated = DateTime.UtcNow
                    };

                    await categoryRepository.AddAsync(category);
                    await unitOfWork.SaveAsync();

                    return Result<CreateCategoryResponse>.Success(new CreateCategoryResponse(category.Id), "Category created successfully!");
                }

                catch (Exception ex)
                {
                    return Result<CreateCategoryResponse>.Failure($"An Error occured: {ex.Message}");
                }
            }
            
        }
        public record CreateCategoryResponse(Guid Id);
    }
}
