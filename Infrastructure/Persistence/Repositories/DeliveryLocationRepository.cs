using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class DeliveryLocationRepository(OrderEaseDbContext context) : IDeliveryLocationRepository
    {
        public async Task AddAsync(DeliveryLocation location)
        {
            await context.DeliveryLocations.AddAsync(location);
        }

        public async Task<DeliveryLocation?> GetAsync(Guid id)
        {
            return await context.DeliveryLocations.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ICollection<DeliveryLocation>> GetAllAsync(bool activeOnly)
        {
            var query = context.DeliveryLocations.AsQueryable();
            if (activeOnly) query = query.Where(x => x.IsActive);
            return await query.OrderBy(x => x.Name).ToListAsync();
        }

        public void Update(DeliveryLocation location)
        {
            context.DeliveryLocations.Update(location);
        }
    }
}
