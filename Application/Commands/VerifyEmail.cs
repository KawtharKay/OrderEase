using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class VerifyEmail
    {
        public record VerifyEmailCommand(string Email, string Code) : IRequest<Result<VerifyEmailResponse>>;

        public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
        {
            public VerifyEmailCommandValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithMessage("Email is required");

                RuleFor(x => x.Code)
                    .NotEmpty()
                    .WithMessage("Verification code is required")
                    .Length(6)
                    .WithMessage("Enter the 6-digit code from your email");
            }
        }

        public class VerifyEmailHandler(IUserRepository userRepository, IUnitOfWork unitOfWork) : IRequestHandler<VerifyEmailCommand, Result<VerifyEmailResponse>>
        {
            public async Task<Result<VerifyEmailResponse>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.Email);
                if (user == null) return Result<VerifyEmailResponse>.Failure("No account found with this email address");

                if (user.IsVerified) return Result<VerifyEmailResponse>.Failure("Email is already verified");

                if (user.VerificationToken != request.Code) return Result<VerifyEmailResponse>.Failure("Incorrect verification code");

                if (user.VerificationTokenExpiry < DateTime.UtcNow) return Result<VerifyEmailResponse>.Failure("This code has expired. Please request a new one");

                user.IsVerified = true;
                user.VerificationToken = null;
                user.VerificationTokenExpiry = null;
                userRepository.Update(user);

                await unitOfWork.SaveAsync();

                return Result<VerifyEmailResponse>.Success(new VerifyEmailResponse(user.Id), "Email verified successfully. You can now log in");
            }
        }

        public record VerifyEmailResponse(Guid Id);
    }
}