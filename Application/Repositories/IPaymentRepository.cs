using Domain.Entities;

namespace Application.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetAsync(Guid id);
        Task<Payment?> GetByPaystackReferenceAsync(string reference);
        Task<ICollection<Payment>> GetAllByCustomerIdAsync(Guid customerId);
        Task<ICollection<Payment>> GetByOrderIdAsync(Guid orderId);
        Task<ICollection<Payment>> GetAllAsync();
        void Update(Payment payment);
    }
}