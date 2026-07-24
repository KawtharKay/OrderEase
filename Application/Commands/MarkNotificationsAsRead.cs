using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using MediatR;

namespace Application.Commands
{
    public class MarkNotificationsAsRead
    {
        public record MarkNotificationsAsReadCommand() : IRequest<Result<string>>;

        public class MarkNotificationsAsReadHandler(INotificationRepository notificationRepository, ICurrentUser currentUser, IUnitOfWork unitOfWork)
            : IRequestHandler<MarkNotificationsAsReadCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(MarkNotificationsAsReadCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var userId = currentUser.GetCurrentUserId();
                    var unread = await notificationRepository.GetUnreadNotificationByUserAsync(userId);

                    foreach (var notification in unread)
                    {
                        notification.IsRead = true;
                        notificationRepository.Update(notification);
                    }

                    await unitOfWork.SaveAsync();
                    return Result<string>.Success("Marked", "Notifications marked as read");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}