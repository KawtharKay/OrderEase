using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ISupplierRepository
    {
        Task AddAsync(Supplier supplier);
        Task<Supplier?> GetAsync(Guid id);
        Task<Supplier?> GetByUserIdAsync(Guid userId);
        void Update(Supplier supplier);
    }
}