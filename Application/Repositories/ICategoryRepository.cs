using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task AddAsync(Category category);
        Task<Category?> GetAsync(Guid id);
        Task<Category?> GetAsync(string name);
        Task<ICollection<Category>> GetAllAsync();
        void Update(Category category);
        void Delete(Category category);
    }
}