using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands
{
    public class ResetPassword
    {
        public record ResetPasswordCommand(string Email, string Token, string NewPassword, string ConfirmPassword) : IRequest<Result<string>>;

        public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
        {
            public ResetPasswordValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithMessage("Email is required")
                    .EmailAddress()
                    .WithMessage("Enter a valid email address");

                RuleFor(x => x.Token)
                    .NotEmpty()
                    .WithMessage("Reset code is required");

                RuleFor(x => x.NewPassword)
                    .NotEmpty()
                    .WithMessage("New password is required")
                    .MinimumLength(6)
                    .WithMessage("Password must be at least 6 characters");

                RuleFor(x => x.ConfirmPassword)
                    .NotEmpty()
                    .WithMessage("Please confirm your password")
                    .Equal(x => x.NewPassword)
                    .WithMessage("Passwords do not match");
            }
        }

        public class ResetPasswordHandler(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, IUnitOfWork unitOfWork) : IRequestHandler<ResetPasswordCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var user = await userRepository.GetAsync(request.Email);
                    if (user == null) return Result<string>.Failure("No account found with this email address");

                    if (user.PasswordResetToken != request.Token) return Result<string>.Failure("Invalid reset code");

                    if (user.ResetTokenExpiry < DateTime.UtcNow) return Result<string>.Failure("Reset code has expired. Please request a new one");

                    string saltedPassword = $"{user.Salt}{request.NewPassword}";
                    user.HashPassword = passwordHasher.HashPassword(user, saltedPassword);

                    user.PasswordResetToken = null;
                    user.ResetTokenExpiry = null;

                    userRepository.Update(user);
                    await unitOfWork.SaveAsync();

                    return Result<string>.Success("Password reset", "Your password has been reset successfully. You can now log in");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}