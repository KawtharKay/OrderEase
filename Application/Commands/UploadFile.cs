using Application.Common.Dtos;
using Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class UploadFile
    {
        public record UploadFileCommand(IFormFile File) : IRequest<Result<UploadFileResponse>>;

        public class UploadFileValidator : AbstractValidator<UploadFileCommand>
        {
            public UploadFileValidator()
            {
                RuleFor(x => x.File)
                    .NotNull()
                    .WithMessage("File is required")
                    .Must(x => x.Length > 0)
                    .WithMessage("File cannot be empty");
            }
        }

        public class UploadFileHandler(IImageUploadService imageUploadService) : IRequestHandler<UploadFileCommand, Result<UploadFileResponse>>
        {
            public async Task<Result<UploadFileResponse>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var imageUrl = await imageUploadService.UploadImageAsync(request.File);

                    return Result<UploadFileResponse>.Success(new UploadFileResponse(imageUrl), "File uploaded successfully");
                }
                catch (Exception ex)
                {
                    return Result<UploadFileResponse>.Failure($"Upload failed: {ex.Message}");
                }
            }
        }

        public record UploadFileResponse(string Url);
    }
}