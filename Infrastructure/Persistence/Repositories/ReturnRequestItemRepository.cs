using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ReturnRequestItemRepository(OrderEaseDbContext context) : IReturnRequestItemRepository
    {
        public async Task AddRangeAsync(ICollection<ReturnRequestItem> items)
        {
            await context.ReturnRequestItems.AddRangeAsync(items);
        }

        public async Task<ICollection<ReturnRequestItem>> GetAllByReturnRequestIdAsync(Guid returnRequestId)
        {
            return await context.ReturnRequestItems
                .Where(x => x.ReturnRequestId == returnRequestId)
                .Include(x => x.Item)
                .ToListAsync();
        }
    }
}