using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using MediatR;

namespace Application.Queries
{
    public class GetMyNotifications
    {
        public record GetMyNotificationsQuery() : IRequest<Result<GetMyNotificationsResponse>>;

        public class GetMyNotificationsHandler(INotificationRepository notificationRepository, ICurrentUser currentUser)
            : IRequestHandler<GetMyNotificationsQuery, Result<GetMyNotificationsResponse>>
        {
            public async Task<Result<GetMyNotificationsResponse>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var userId = currentUser.GetCurrentUserId();
                    var notifications = await notificationRepository.GetAllByUserIdAsync(userId);

                    var items = notifications.Select(n => new NotificationItem(
                        n.Id, n.Title, n.Message, n.NotificationType.ToString(), n.ReferenceId, n.IsRead, n.DateCreated)).ToList();

                    var unreadCount = items.Count(x => !x.IsRead);

                    return Result<GetMyNotificationsResponse>.Success(
                        new GetMyNotificationsResponse(unreadCount, items),
                        "Notifications retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetMyNotificationsResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record NotificationItem(Guid Id, string Title, string Message, string NotificationType, Guid? ReferenceId, bool IsRead, DateTime DateCreated);
        public record GetMyNotificationsResponse(int UnreadCount, ICollection<NotificationItem> Notifications);
    }
}