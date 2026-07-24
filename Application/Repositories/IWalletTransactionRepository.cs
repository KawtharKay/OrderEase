using Domain.Entities;

public interface IWalletTransactionRepository
{
    Task AddAsync(WalletTransaction transaction);
    Task<WalletTransaction?> GetByReferenceAsync(string reference);
    Task<IEnumerable<WalletTransaction>> GetByWalletAsync(Guid walletId);
    void Update(WalletTransaction transaction);
}