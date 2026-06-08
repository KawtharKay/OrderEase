using Domain.Entities;

namespace Application.Repositories
{
    public interface IUserRepository
    {
        Task<bool> IsExistAsync(string email);
        Task AddAsync(User user);
        Task<User?> GetAsync(Guid id);
        Task<User?> GetAsync(string email);
        Task<ICollection<User>> GetAllAsync();
        void Update(User user);
    }
}
