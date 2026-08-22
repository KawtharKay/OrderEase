using Application.Common.Dtos;
using Application.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetAllOrders
    {
        public record GetAllOrdersQuery() : IRequest<Result<ICollection<GetAllOrdersResponse>>>;

        public class GetAllOrdersHandler(IOrderRepository orderRepository) : IRequestHandler<GetAllOrdersQuery, Result<ICollection<GetAllOrdersResponse>>>
        {
            public async Task<Result<ICollection<GetAllOrdersResponse>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var orders = await orderRepository.GetAllAsync();
                    var response = orders.Select(o => new GetAllOrdersResponse(
                        o.Id, o.OrderNumber, o.Customer.Name, o.OrderStatus.ToString(), o.TotalPrice, o.OrderDate,
                        OrderStatusTransitions.GetAllowedNextStatuses(o.OrderStatus).Select(s => s.ToString()).ToList()))
                        .ToList();

                    return Result<ICollection<GetAllOrdersResponse>>.Success(response, "Orders retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetAllOrdersResponse>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetAllOrdersResponse(Guid Id, string OrderNumber, string CustomerName, string OrderStatus, decimal TotalPrice, DateTime OrderDate, ICollection<string> AllowedNextStatuses);
    }
}