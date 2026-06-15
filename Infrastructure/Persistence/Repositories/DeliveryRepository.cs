using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class DeliveryRepository(OrderEaseDbContext context) : IDeliveryRepository
    {
        public async Task AddAsync(Delivery delivery)
        {
            await context.Deliveries.AddAsync(delivery);
        }

        public async Task<Delivery?> GetAsync(Guid id)
        {
            return await context.Deliveries.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Delivery?> GetByOrderIdAsync(Guid orderId)
        {
            return await context.Deliveries.FirstOrDefaultAsync(x => x.OrderId == orderId);
        }

        public void Update(Delivery delivery)
        {
            context.Deliveries.Update(delivery);
        }
    }
}