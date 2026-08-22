using Domain.Entities;

namespace Application.Repositories
{
    public interface IItemRepository
    {
        Task AddAsync(Item item);
        Task<Item?> GetAsync(Guid id);
        Task<ICollection<Item>> GetByCategoryIdAsync(Guid categoryId);
        Task<ICollection<Item>> GetAllAsync();
        Task<(ICollection<Item> Items, int TotalCount)> SearchAsync(string? search, Guid? categoryId, int page, int pageSize);
        void Update(Item item);
    }
}