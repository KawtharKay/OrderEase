using Application.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories
{
    public class UnitOfWork(OrderEaseDbContext context) : IUnitOfWork
    {
        public async Task<int> SaveAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}
