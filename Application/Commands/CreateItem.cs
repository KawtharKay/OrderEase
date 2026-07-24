using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Commands
{
    public class CreateItem
    {
        public record CreateItemCommand(Guid CategoryId, string Title, string ImageUrl, decimal Price, int Quantity) : IRequest<Result<CreateItemResponse>>;

        public class CreateItemValidator : AbstractValidator<CreateItemCommand>
        {
            public CreateItemValidator()
            {
                RuleFor(x => x.CategoryId)
                    .NotEmpty()
                    .WithMessage("Category is required");

                RuleFor(x => x.Title)
                    .NotEmpty()
                    .WithMessage("Title is required")
                    .MaximumLength(100)
                    .WithMessage("Title should not exceed 100 characters");

                RuleFor(x => x.ImageUrl)
                    .NotEmpty()
                    .WithMessage("Image URL is required")
                    .MaximumLength(200)
                    .WithMessage("Image URL cannot exceed 200 characters");

                RuleFor(x => x.Price)
                    .GreaterThan(0)
                    .WithMessage("Price must be greater than zero");

                RuleFor(x => x.Quantity)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Quantity cannot be negative");
            }
        }
        public class CreateItemHandler(IItemRepository itemRepository, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateItemCommand, Result<CreateItemResponse>>
        {
            public async Task<Result<CreateItemResponse>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var category = await categoryRepository.GetAsync(request.CategoryId);
                    if (category is null) return Result<CreateItemResponse>.Failure("Category not found");

                    var item = new Item
                    {
                        CategoryId = request.CategoryId,
                        Title = request.Title,
                        ImageUrl = request.ImageUrl,
                        Price = request.Price,
                        Quantity = request.Quantity,
                        IsAvailable = request.Quantity > 0,
                        DateCreated = DateTime.UtcNow
                    };

                    await itemRepository.AddAsync(item);
                    await unitOfWork.SaveAsync();

                    return Result<CreateItemResponse>.Success(item.Adapt<CreateItemResponse>(), "Item created successfully!");
                }
                catch (Exception ex)
                {
                    return Result<CreateItemResponse>.Failure($"An error occured: {ex.Message}");
                }
            }
        }

        public record CreateItemResponse(Guid Id, string Title, decimal Price, int Quantity, bool IsAvailable);
    }
}
