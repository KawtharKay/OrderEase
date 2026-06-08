using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer);
        Task<Customer?> GetAsync(Guid id);
        Task<Customer?> GetByUserIdAsync(Guid userId);
        Task<Customer?> GetAsync(string email);
        Task<ICollection<Customer>> GetAllAsync();
        void Update(Customer customer);
    }
}