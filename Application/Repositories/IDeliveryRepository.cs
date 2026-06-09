using Domain.Entities;

namespace Application.Repositories
{
    public interface IDeliveryRepository
    {
        Task AddAsync(Delivery delivery);
        Task<Delivery?> GetAsync(Guid id);
        Task<Delivery?> GetByOrderIdAsync(Guid orderId);
        void Update(Delivery delivery);
    }
}