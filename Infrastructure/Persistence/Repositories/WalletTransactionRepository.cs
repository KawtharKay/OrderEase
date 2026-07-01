using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class WalletTransactionRepository(OrderEaseDbContext context) : IWalletTransactionRepository
    {
        public async Task AddAsync(WalletTransaction transaction)
        {
            await context.WalletTransactions.AddAsync(transaction);
        }

        public async Task<WalletTransaction?> GetByReferenceAsync(string reference)
        {
            return await context.WalletTransactions
                .FirstOrDefaultAsync(x => x.PaystackReference == reference && !x.IsDeleted);
        }

        public async Task<IEnumerable<WalletTransaction>> GetByWalletAsync(Guid walletId)
        {
            return await context.WalletTransactions
                .Where(x => x.WalletId == walletId && !x.IsDeleted)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public void Update(WalletTransaction transaction)
        {
            context.WalletTransactions.Update(transaction);
        }
    }
}