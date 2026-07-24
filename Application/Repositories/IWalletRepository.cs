using Domain.Entities;

public interface IWalletRepository
{
    Task AddAsync(Wallet wallet);
    Task<Wallet?> GetAsync(Guid id);
    Task<Wallet?> GetByCustomerAsync(Guid customerId);
    void Update(Wallet wallet);
}