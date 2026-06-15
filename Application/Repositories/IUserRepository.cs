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
        Task AssignRoleAsync(UserRole userRole);
        void Update(User user);
        Task<User?> GetByVerificationTokenAsync(string token);
    }
}
