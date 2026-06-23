using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services
{
    public class NotificationService(IHubContext<NotificationHub> hubContext, INotificationRepository notificationRepository, IUnitOfWork unitOfWork) : INotificationService
    {
        public async Task SendNotificationAsync(Guid userId, string title, string message, string notificationType, Guid? referenceId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = Enum.Parse<NotificationType>(notificationType),
                ReferenceId = referenceId,
                IsRead = false,
                DateCreated = DateTime.UtcNow
            };

            await notificationRepository.AddAsync(notification);
            await unitOfWork.SaveAsync();

            // Push live to the user's browser if connected
            await hubContext.Clients.Group(userId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Title,
                    notification.Message,
                    notification.NotificationType,
                    notification.ReferenceId,
                    notification.DateCreated
                });
        }
    }
}