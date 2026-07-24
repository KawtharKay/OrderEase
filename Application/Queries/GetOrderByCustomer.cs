using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetOrdersByCustomer
    {
        public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<Result<ICollection<GetOrdersByCustomerResponse>>>;

        public class GetOrdersByCustomerValidator : AbstractValidator<GetOrdersByCustomerQuery>
        {
            public GetOrdersByCustomerValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer ID is required");
            }
        }

        public class GetOrdersByCustomerHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrdersByCustomerQuery, Result<ICollection<GetOrdersByCustomerResponse>>>
        {
            public async Task<Result<ICollection<GetOrdersByCustomerResponse>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var orders = await orderRepository.GetAllByCustomerIdAsync(request.CustomerId);
                    return Result<ICollection<GetOrdersByCustomerResponse>>.Success(orders.Adapt<ICollection<GetOrdersByCustomerResponse>>(), "Orders retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetOrdersByCustomerResponse>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetOrdersByCustomerResponse(Guid Id, string OrderNumber, string OrderStatus, decimal TotalPrice, DateTime OrderDate);
    }
}