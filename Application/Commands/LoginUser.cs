using Application.Repositories;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands
{
    public class LoginUser
    {
        public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResponse>;

        public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
        {
            public LoginUserCommandValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithMessage("Email is required")
                    .EmailAddress()
                    .WithMessage("Enter a valid email address");

                RuleFor(x => x.Password)
                    .NotEmpty()
                    .WithMessage("Password required")
                    .MinimumLength(6)
                    .WithMessage("Password must be at least 6 characters");
            }
        }

        public class LoginUserHandler(IUserRepository userRepository, ITokenService tokenService, IPasswordHasher<User> passwordHasher) : IRequestHandler<LoginUserCommand, LoginUserResponse>
        {
            public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.Email);
                if (user is null) throw new Exception("Invalid credentials");

                if (!user.IsVerified) throw new Exception("Please verify your email address");

                string hashPassword = $"{user.Salt}{request.Password}";
                var verifyPassword = passwordHasher.VerifyHashedPassword(user, user.HashPassword, hashPassword);
                if (verifyPassword == PasswordVerificationResult.Failed) throw new Exception("Invalid credentials");

                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                var token = tokenService.GenerateToken(new LoginResponse(user.Id, user.Email, roles));
                return new LoginUserResponse(user.Id, token);
            }
        }
        public record LoginUserResponse(Guid Id, string Token);
        public record LoginResponse(Guid Id, string Email, List<string> Roles);
    }
}
