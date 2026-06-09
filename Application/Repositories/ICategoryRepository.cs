using Domain.Entities;

namespace Application.Repositories
{
    public interface ICategoryRepository
    {
        Task AddAsync(Category category);
        Task<Category?> GetAsync(Guid id);
        Task<Category?> GetAsync(string name);
        Task<ICollection<Category>> GetAllAsync();
        void Update(Category category);
    }
}