using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class DeleteItem
    {
        public record DeleteItemCommand(Guid Id) : IRequest<Result<string>>;

        public class DeleteItemValidator : AbstractValidator<DeleteItemCommand>
        {
            public DeleteItemValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Item Id is required");
            }
        }
        public class DeleteItemHandler(IItemRepository itemRepository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteItemCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var item = await itemRepository.GetAsync(request.Id);
                    if (item is null) return Result<string>.Failure("Item not found");

                    item.IsDeleted = true;
                    await unitOfWork.SaveAsync();

                    return Result<string>.Success("Deleted", "Item deleted successfully!");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occured: {ex.Message}");
                }
            }
        }
    }
}
