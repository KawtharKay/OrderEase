using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ItemRepository(OrderEaseDbContext context) : IItemRepository
    {
        public async Task AddAsync(Item item)
        {
            await context.Items.AddAsync(item);
        }

        public async Task<Item?> GetAsync(Guid id)
        {
            return await context.Items.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ICollection<Item>> GetAllAsync()
        {
            return await context.Items.ToListAsync();
        }


        public async Task<ICollection<Item>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await context.Items.Where(x => x.CategoryId == categoryId).ToListAsync();
        }

        public void Update(Item item)
        {
            context.Items.Update(item);
        }
    }
}