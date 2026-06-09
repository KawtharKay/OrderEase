using Domain.Entities;

namespace Application.Repositories
{
    public interface IReturnRequestRepository
    {
        Task AddAsync(ReturnRequest returnRequest);
        Task<ReturnRequest?> GetAsync(Guid id);
        Task<ICollection<ReturnRequest>> GetAllAsync();
        Task<ICollection<ReturnRequest>> GetAllByCustomerIdAsync(Guid customerId);
        void Update(ReturnRequest returnRequest);
    }
}