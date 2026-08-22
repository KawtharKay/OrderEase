using Domain.Entities;

namespace Application.Repositories
{
    public interface IStockMovementRepository
    {
        Task AddAsync(StockMovement movement);
        Task<ICollection<StockMovement>> GetByItemIdAsync(Guid itemId);
    }
}
