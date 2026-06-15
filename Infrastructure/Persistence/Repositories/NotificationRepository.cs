using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class NotificationRepository(OrderEaseDbContext context) : INotificationRepository
    {
        public async Task AddAsync(Notification notification)
        {
            await context.Notifications.AddAsync(notification);
        }

        public async Task<Notification?> GetAsync(Guid id)
        {
            return await context.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ICollection<Notification>> GetAllByUserIdAsync(Guid userId)
        {
            return await context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public async Task<ICollection<Notification>> GetUnreadNotificationByUserAsync(Guid userId)
        {
            return await context.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public void Update(Notification notification)
        {
            context.Notifications.Update(notification);
        }
    }
}