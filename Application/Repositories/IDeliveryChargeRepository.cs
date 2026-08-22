using Domain.Entities;

namespace Application.Repositories
{
    public interface IDeliveryChargeRepository
    {
        Task AddRangeAsync(ICollection<DeliveryCharge> charges);
        Task<ICollection<DeliveryCharge>> GetByOrderIdAsync(Guid orderId);
        Task RemoveAllForOrderAsync(Guid orderId);
    }
}
