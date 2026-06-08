using Domain.Entities;

namespace Application.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task<Order?> GetAsync(Guid id);
        Task<Order?> GetAsync(string orderNumber);
        Task<Order?> GetByOrderIdAsync(Guid orderId);
        Task<ICollection<Order>> GetAllAsync();
        Task<ICollection<Order>> GetAllByCustomerIdAsync(Guid customerId);
        void Update(Order order);
    }
}
