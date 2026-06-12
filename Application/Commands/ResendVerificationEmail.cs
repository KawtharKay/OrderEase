using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class ResendVerificationEmail
    {
        public record ResendVerificationEmailCommand(string Email) : IRequest<Result<string>>;

        public class ResendVerificationEmailValidator : AbstractValidator<ResendVerificationEmailCommand>
        {
            public ResendVerificationEmailValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required")
                    .EmailAddress().WithMessage("Enter a valid email address");
            }
        }

        public class ResendVerificationEmailHandler(IUserRepository userRepository, IEmailService emailService, IUnitOfWork unitOfWork) : IRequestHandler<ResendVerificationEmailCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.Email);
                if (user == null) return Result<string>.Failure("No account found with this email address");

                if (user.IsVerified) return Result<string>.Failure("This account is already verified");

                user.VerificationToken = Guid.NewGuid().ToString();
                user.VerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
                userRepository.Update(user);
                await unitOfWork.SaveAsync();

                await emailService.SendVerificationEmailAsync(request.Email, user.VerificationToken);

                return Result<string>.Success("Verification email sent", "Please check your email for the verification link");
            }
        }
    }
}