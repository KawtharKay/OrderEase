using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class VerifyEmail
    {
        public record VerifyEmailCommand(string Token) : IRequest<Result<VerifyEmailResponse>>;

        public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
        {
            public VerifyEmailCommandValidator()
            {
                RuleFor(x => x.Token)
                    .NotEmpty()
                    .WithMessage("Verification token is required");
            }
        }

        public class VerifyEmailHandler(IUserRepository userRepository, IUnitOfWork unitOfWork) : IRequestHandler<VerifyEmailCommand, Result<VerifyEmailResponse>>
        {
            public async Task<Result<VerifyEmailResponse>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetByVerificationTokenAsync(request.Token);
                if (user == null) return Result<VerifyEmailResponse>.Failure("Invalid verification token");

                if (user.VerificationTokenExpiry < DateTime.UtcNow) return Result<VerifyEmailResponse>.Failure("Verification token has expired. Please register again");

                if (user.IsVerified) return Result<VerifyEmailResponse>.Failure("Email is already verified");

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