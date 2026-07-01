using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class UpdateOrderStatus
    {
        public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<Result<string>>;

        public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
        {
            public UpdateOrderStatusValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order ID is required");

                RuleFor(x => x.Status)
                    .IsInEnum()
                    .WithMessage("Invalid order status");
            }
        }

        public class UpdateOrderStatusHandler(IOrderRepository orderRepository, INotificationService notificationService, IUnitOfWork unitOfWork) : IRequestHandler<UpdateOrderStatusCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetAsync(request.OrderId);
                    if (order is null) return Result<string>.Failure("Order not found");

                    order.OrderStatus = request.Status;
                    orderRepository.Update(order);
                    await unitOfWork.SaveAsync();

                    await notificationService.SendNotificationAsync(order.Customer.UserId, "Order Status Updated", $"Your order {order.OrderNumber} is now {request.Status}",
                        "OrderStatusChanged", order.Id);

                    return Result<string>.Success("Updated", $"Order status updated to {request.Status}");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}