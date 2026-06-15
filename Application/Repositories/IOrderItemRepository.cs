using Domain.Entities;

namespace Application.Repositories
{
    public interface IOrderItemRepository
    {
        Task AddRangeAsync(ICollection<OrderItem> orderItems);
        Task<ICollection<OrderItem>> GetAllByOrderIdAsync(Guid orderId);
    }
}