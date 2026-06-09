using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class SupplierRepository(OrderEaseDbContext context) : ISupplierRepository
    {
        public async Task AddAsync(Supplier supplier)
        {
            await context.Suppliers.AddAsync(supplier);
        }

        public async Task<Supplier?> GetAsync(Guid id)
        {
            return await context.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Supplier?> GetByUserIdAsync(Guid userId)
        {
            return await context.Suppliers.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public void Update(Supplier supplier)
        {
            context.Suppliers.Update(supplier);
        }
    }
}