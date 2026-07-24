using Domain.Entities;

namespace Application.Repositories
{
    public interface ISupplierRepository
    {
        Task AddAsync(Supplier supplier);
        Task<Supplier?> GetAsync(Guid id);
        Task<Supplier?> GetByUserIdAsync(Guid userId);
        Task<Supplier?> GetFirstAsync();
        void Update(Supplier supplier);
    }
}