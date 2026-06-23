namespace Application.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Guid userId, string title, string message, string notificationType, Guid? referenceId = null);
    }
}