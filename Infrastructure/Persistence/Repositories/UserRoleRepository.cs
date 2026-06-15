using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRoleRepository(OrderEaseDbContext context) : IUserRoleRepository
    {
        public async Task AddAsync(UserRole userRole)
        {
            await context.UserRoles.AddAsync(userRole);
        }

        public async Task<bool> IsExistAsync(Guid userId, Guid roleId)
        {
            return await context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }
    }
}
