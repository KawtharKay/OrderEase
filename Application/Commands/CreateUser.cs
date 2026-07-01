using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Commands
{
    public class CreateUser
    {
        public record CreateUserCommand(string Email, string Password) : IRequest<Result<CreateUserResponse>>;

        public class CreateUserValidator : AbstractValidator<CreateUserCommand>
        {
            public CreateUserValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithMessage("Email is required")
                    .EmailAddress()
                    .WithMessage("Enter a valid email address")
                    .MaximumLength(70)
                    .WithMessage("Email should not exceed 70 characters");

                RuleFor(x => x.Password)
                    .NotEmpty()
                    .WithMessage("Password is required")
                    .MinimumLength(6)
                    .WithMessage("Password must be at least 6 characters");
            }
        }

        public class CreateUserHandler(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, IEmailService emailService, ILogger<CreateUserHandler> logger, IUnitOfWork unitOfWork) 
            : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
        {
            public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
            {
                var userExists = await userRepository.IsExistAsync(request.Email);
                if (userExists) return Result<CreateUserResponse>.Failure("User already exist");

                string salt = Guid.NewGuid().ToString();
                string verificationToken = Guid.NewGuid().ToString();

                var user = new User
                {
                    Email = request.Email,
                    Salt = salt,
                    CreatedBy = request.Email,
                    IsVerified = false,
                    VerificationToken = verificationToken,
                    VerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                    DateCreated = DateTime.UtcNow
                };
                string saltPassword = $"{salt}{request.Password}";
                user.HashPassword = passwordHasher.HashPassword(user, saltPassword);

                await userRepository.AddAsync(user);
                await unitOfWork.SaveAsync();

                try
                {
                    await emailService.SendVerificationEmailAsync(request.Email, verificationToken);
                }
                
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send verification email to {Email}", request.Email);
                    return Result<CreateUserResponse>.Success(new CreateUserResponse(user.Id), "Account created but verification email could not be sent. Please use the resend verification option");
                }

                return Result<CreateUserResponse>.Success(new CreateUserResponse(user.Id), "User created successfully! Please check your email to verify your account");
            }
        }

        public record CreateUserResponse(Guid Id);
        
    }
}