using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IReturnRequestRepository
    {
        Task AddAsync(ReturnRequest returnRequest);
        Task<ReturnRequest?> GetAsync(Guid id);
        Task<ReturnRequest?> GetWithItemsAsync(Guid returnRequestId);
        Task<ICollection<ReturnRequest>> GetAllAsync();
        Task<ICollection<ReturnRequest>> GetByCustomerIdAsync(Guid customerId);
        void Update(ReturnRequest returnRequest);
    }
}