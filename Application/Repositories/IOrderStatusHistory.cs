using Domain.Entities;

namespace Application.Repositories
{
    public interface IOrderStatusHistoryRepository
    {
        Task AddAsync(OrderStatusHistory history);
        Task<ICollection<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId);
    }
}