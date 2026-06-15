namespace Application.Services
{
    public interface ICurrentUser
    {
        Guid GetCurrentUserId();
        string GetCurrentUserEmail();
        bool IsInRole(string role);
    }
}