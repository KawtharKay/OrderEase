using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CategoryRepository(OrderEaseDbContext context) : ICategoryRepository
    {
        public async Task AddAsync(Category category)
        {
            await context.Categories.AddAsync(category);
        }

        public async Task<Category?> GetAsync(Guid id)
        {
            return await context.Categories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<Category?> GetAsync(string name)
        {
            return await context.Categories.FirstOrDefaultAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<ICollection<Category>> GetAllAsync()
        {
            return await context.Categories.ToListAsync();
        }

        public void Update(Category category)
        {
            context.Categories.Update(category);
        }
    }
}