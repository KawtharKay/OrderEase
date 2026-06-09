using Domain.Entities;

namespace Application.Repositories
{
    public interface IItemRepository
    {
        Task AddAsync(Item item);
        Task<Item?> GetAsync(Guid id);
        Task<ICollection<Item>> GetByCategoryIdAsync(Guid categoryId);
        Task<ICollection<Item>> GetAllAsync();
        void Update(Item item);
    }
}