using Domain.Entities;

namespace Application.Repositories
{
    public interface IDeliveryLocationRepository
    {
        Task AddAsync(DeliveryLocation location);
        Task<DeliveryLocation?> GetAsync(Guid id);
        Task<ICollection<DeliveryLocation>> GetAllAsync(bool activeOnly);
        void Update(DeliveryLocation location);
    }
}
