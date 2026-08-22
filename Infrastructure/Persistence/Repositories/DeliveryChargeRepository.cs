using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class DeliveryChargeRepository(OrderEaseDbContext context) : IDeliveryChargeRepository
    {
        public async Task AddRangeAsync(ICollection<DeliveryCharge> charges)
        {
            await context.DeliveryCharges.AddRangeAsync(charges);
        }

        public async Task<ICollection<DeliveryCharge>> GetByOrderIdAsync(Guid orderId)
        {
            return await context.DeliveryCharges
                .Where(x => x.OrderId == orderId && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task RemoveAllForOrderAsync(Guid orderId)
        {
            var existing = await context.DeliveryCharges.Where(x => x.OrderId == orderId).ToListAsync();
            context.DeliveryCharges.RemoveRange(existing);
        }
    }
}
