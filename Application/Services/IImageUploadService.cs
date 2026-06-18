using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}