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
            return await context.Items.Where(x => !x.IsDeleted).ToListAsync();
        }


        public async Task<ICollection<Item>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await context.Items.Where(x => x.CategoryId == categoryId).ToListAsync();
        }

        public async Task<(ICollection<Item> Items, int TotalCount)> SearchAsync(string? search, Guid? categoryId, int page, int pageSize)
        {
            var query = context.Items.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Title.Contains(search) || x.Category.Name.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(x => x.CategoryId == categoryId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public void Update(Item item)
        {
            context.Items.Update(item);
        }
    }
}