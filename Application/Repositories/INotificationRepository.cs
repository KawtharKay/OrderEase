using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<Notification?> GetAsync(Guid id);
        Task<ICollection<Notification>> GetByUserIdAsync(Guid userId);
        void Update(Notification notification);
    }
}