using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetOrderById
    {
        public record GetOrderByIdQuery(Guid Id) : IRequest<Result<GetOrderByIdResponse>>;

        public class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>
        {
            public GetOrderByIdValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Order ID is required");
            }
        }

        public class GetOrderByIdHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
        {
            public async Task<Result<GetOrderByIdResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetAsync(request.Id);
                    if (order == null) return Result<GetOrderByIdResponse>.Failure("Order not found");

                    return Result<GetOrderByIdResponse>.Success(order.Adapt<GetOrderByIdResponse>(), "Order retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetOrderByIdResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record OrderItemDto(Guid ItemId, string Title, int Quantity, decimal UnitPrice, decimal SubTotal);

        public record GetOrderByIdResponse(Guid Id, string OrderNumber, Guid CustomerId, string OrderStatus, decimal TotalPrice, DateTime OrderDate, ICollection<OrderItemDto> OrderItems);
    }
}