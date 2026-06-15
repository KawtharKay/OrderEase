using Application.Repositories;
using Application.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Authentication
{
    public class CurrentUser(IHttpContextAccessor context) : ICurrentUser
    {
        public Guid GetCurrentUserId()
        {
            var httpContext = context.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("No HTTP context available");

            var claim = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claim))
                throw new InvalidOperationException("User ID claim not found in token");

            if (!Guid.TryParse(claim, out var userId))
                throw new InvalidOperationException("User ID claim is not a valid GUID");

            return userId;
        }

        public string GetCurrentUserEmail()
        {
            var claim = context.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(claim))
                throw new InvalidOperationException("Email claim not found in token");

            return claim;
        }

        public bool IsInRole(string role)
        {
            return context.HttpContext?.User?.IsInRole(role) ?? false;
        }
    }
}