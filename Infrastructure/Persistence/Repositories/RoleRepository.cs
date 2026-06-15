using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RoleRepository(OrderEaseDbContext context) : IRoleRepository
    {
        public async Task AddAsync(Role role)
        {
            await context.Roles.AddAsync(role);
        }

        public async Task<Role?> GetAsync(Guid id)
        {
            return await context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role?> GetAsync(string name)
        {
            return await context.Roles.Include(r => r.UserRoles).FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<bool> IsExistAsync(string name)
        {
            return await context.Roles.AnyAsync(r => r.Name == name);
        }

        public async Task<ICollection<Role>> GetAllAsync()
        {
            return await context.Roles.ToListAsync();
        }
    }
}
