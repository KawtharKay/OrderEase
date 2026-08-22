using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class StockMovementRepository(OrderEaseDbContext context) : IStockMovementRepository
    {
        public async Task AddAsync(StockMovement movement)
        {
            await context.StockMovements.AddAsync(movement);
        }

        public async Task<ICollection<StockMovement>> GetByItemIdAsync(Guid itemId)
        {
            return await context.StockMovements
                .Where(x => x.ItemId == itemId && !x.IsDeleted)
                .OrderByDescending(x => x.DateReceived)
                .ToListAsync();
        }
    }
}
