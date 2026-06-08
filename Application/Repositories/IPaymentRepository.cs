using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetByIdAsync(Guid id);
        Task<Payment?> GetByPaystackReferenceAsync(string reference);
        Task<ICollection<Payment>> GetAllByCustomerIdAsync(Guid customerId);
        Task<ICollection<Payment>> GetAllAsync();
        void Update(Payment payment);
    }
}