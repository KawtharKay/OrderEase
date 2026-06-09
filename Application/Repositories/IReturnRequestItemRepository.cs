using Domain.Entities;

namespace Application.Repositories
{
    public interface IReturnRequestItemRepository
    {
        Task AddRangeAsync(ICollection<ReturnRequestItem> items);
        Task<ICollection<ReturnRequestItem>> GetAllByReturnRequestIdAsync(Guid returnRequestId);
    }
}