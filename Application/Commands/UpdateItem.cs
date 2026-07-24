using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Commands
{
    public class UpdateItem
    {
        public record UpdateItemCommand(Guid Id, string Title, string ImageUrl, decimal Price, int Quantity) : IRequest<Result<UpdateItemResponse>>;

        public class UpdateItemValidator : AbstractValidator<UpdateItemCommand>
        {
            public UpdateItemValidator()
            {
                RuleFor(x => x.Id)
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

        public class UpdateItemHandler(IItemRepository itemRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateItemCommand, Result<UpdateItemResponse>>
        {
            public async Task<Result<UpdateItemResponse>> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var item = await itemRepository.GetAsync(request.Id);
                    if (item is null) return Result<UpdateItemResponse>.Failure("Item not found");

                    item.Title = request.Title;
                    item.ImageUrl = request.ImageUrl;
                    item.Price = request.Price;
                    item.Quantity = request.Quantity;
                    item.IsAvailable = request.Quantity > 0;

                    itemRepository.Update(item);
                    await unitOfWork.SaveAsync();

                    return Result<UpdateItemResponse>.Success(item.Adapt<UpdateItemResponse>(), "Item updated successfully!");
                }
                catch (Exception ex)
                {
                    return Result<UpdateItemResponse>.Failure($"An error occured: {ex.Message}");
                }
            }
        }

        public record UpdateItemResponse(Guid Id, string Title, string ImageUrl, decimal Price, int Quantity, bool IsAvailable);
    }
}
