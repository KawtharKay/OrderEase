using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class WalletRepository(OrderEaseDbContext context) : IWalletRepository
    {
        public async Task AddAsync(Wallet wallet)
        {
            await context.Wallets.AddAsync(wallet);
        }

        public async Task<Wallet?> GetAsync(Guid id)
        {
            return await context.Wallets
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<Wallet?> GetByCustomerAsync(Guid customerId)
        {
            return await context.Wallets
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && !x.IsDeleted);
        }

        public void Update(Wallet wallet)
        {
            context.Wallets.Update(wallet);
        }
    }
}