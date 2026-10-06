using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderStatusHistoryRepository(OrderEaseDbContext context) : IOrderStatusHistoryRepository
    {
        public async Task AddAsync(OrderStatusHistory history)
        {
            await context.OrderStatusHistories.AddAsync(history);
        }

        public async Task<ICollection<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId)
        {
            return await context.OrderStatusHistories
                .Where(x => x.OrderId == orderId && !x.IsDeleted)
                .OrderBy(x => x.ChangedAt)
                .ToListAsync();
        }
    }
}
