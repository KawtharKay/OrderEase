using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class ForgotPassword
    {
        public record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;

        public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
        {
            public ForgotPasswordValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithMessage("Email is required")
                    .EmailAddress()
                    .WithMessage("Enter a valid email address");
            }
        }

        public class ForgotPasswordHandler(IUserRepository userRepository, IEmailService emailService, IUnitOfWork unitOfWork) : IRequestHandler<ForgotPasswordCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var user = await userRepository.GetAsync(request.Email);
                    if (user == null) return Result<string>.Failure("No account found with this email address");

                    if (!user.IsVerified) return Result<string>.Failure("Please verify your email address first");

                    var resetToken = new Random().Next(000000, 999999).ToString();

                    user.PasswordResetToken = resetToken;
                    user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
                    userRepository.Update(user);
                    await unitOfWork.SaveAsync();

                    await emailService.SendPasswordResetEmailAsync(request.Email, resetToken);

                    return Result<string>.Success("Reset code sent", "A password reset code has been sent to your email address");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}