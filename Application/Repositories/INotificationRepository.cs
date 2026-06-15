using Domain.Entities;

namespace Application.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<Notification?> GetAsync(Guid id);
        Task<ICollection<Notification>> GetAllByUserIdAsync(Guid userId);
        Task<ICollection<Notification>> GetUnreadNotificationByUserAsync(Guid userId);
        void Update(Notification notification);
    }
}