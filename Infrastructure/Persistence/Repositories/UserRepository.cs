using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository(OrderEaseDbContext context) : IUserRepository
    {
        public async Task<bool> IsExistAsync(string email)
        {
            return await context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).AnyAsync(x => x.Email == email);
        }
        public async Task AddAsync(User user)
        {
            await context.Users.AddAsync(user);
        }

        public async Task<User?> GetAsync(Guid id)
        {
            return await context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> GetAsync(string email)
        {
            return await context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<ICollection<User>> GetAllAsync()
        {
            return await context.Users.ToListAsync();
        }

        public async Task AssignRoleAsync(UserRole userRole)
        {
            var exists = await context.UserRoles.AnyAsync(x => x.UserId == userRole.UserId && x.RoleId == userRole.RoleId);

            if (!exists) await context.UserRoles.AddAsync(userRole);
        }

        public void Update(User user)
        {
            context.Users.Update(user);
        }

        public async Task<User?> GetByVerificationTokenAsync(string token)
        {
            return await context.Users.FirstOrDefaultAsync(x => x.VerificationToken == token);
        }
    }
}