using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static Application.Commands.LoginUser.LoginUserHandler;

namespace Application.Commands
{
    public class LoginUser
    {
        public record LoginUserCommand(string Email, string Password) : IRequest<Result<LoginUserResponse>>;

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

        public class LoginUserHandler(IUserRepository userRepository, ITokenService tokenService, IPasswordHasher<User> passwordHasher) : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
        {
            public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var user = await userRepository.GetAsync(request.Email);
                    if (user is null) return Result<LoginUserResponse>.Failure("Invalid credentials");

                    if (!user.IsVerified) return Result<LoginUserResponse>.Failure("Please verify your email address");

                    string hashPassword = $"{user.Salt}{request.Password}";
                    var verifyPassword = passwordHasher.VerifyHashedPassword(user, user.HashPassword, hashPassword);
                    if (verifyPassword == PasswordVerificationResult.Failed) return Result<LoginUserResponse>.Failure("Invalid credentials");

                    var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                    var token = tokenService.GenerateToken(new LoginResponse(user.Id, user.Email, roles));

                    return Result<LoginUserResponse>.Success(new LoginUserResponse(user.Id, token), "Login successful!");
                }
                catch (Exception ex)
                {
                    return Result<LoginUserResponse>.Failure($"An error occurred while processing the request: {ex.Message}");
                }
            }
        }
            public record LoginUserResponse(Guid Id, string Token);
            public record LoginResponse(Guid Id, string Email, List<string> Roles);
    }
}
