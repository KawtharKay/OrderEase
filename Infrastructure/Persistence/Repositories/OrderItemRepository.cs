using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderItemRepository(OrderEaseDbContext context) : IOrderItemRepository
    {
        public async Task AddRangeAsync(ICollection<OrderItem> orderItems)
        {
            await context.OrderItems.AddRangeAsync(orderItems);
        }

        public async Task<ICollection<OrderItem>> GetAllByOrderIdAsync(Guid orderId)
        {
            return await context.OrderItems
                .Where(x => x.OrderId == orderId)
                .Include(x => x.Item)
                .ToListAsync();
        }
    }
}