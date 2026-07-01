using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class FundWallet
    {
        public record FundWalletCommand(Guid CustomerId, decimal Amount) : IRequest<Result<FundWalletResponse>>;

        public class FundWalletValidator : AbstractValidator<FundWalletCommand>
        {
            public FundWalletValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer is required");

                RuleFor(x => x.Amount)
                    .GreaterThan(0)
                    .WithMessage("Amount must be greater than zero");
            }
        }

        public class FundWalletHandler(
            ICustomerRepository customerRepository,
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IPaystackService paystackService,
            IUnitOfWork unitOfWork) : IRequestHandler<FundWalletCommand, Result<FundWalletResponse>>
        {
            public async Task<Result<FundWalletResponse>> Handle(FundWalletCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetAsync(request.CustomerId);
                    if (customer == null) return Result<FundWalletResponse>.Failure("Customer not found");

                    var wallet = await walletRepository.GetByCustomerAsync(request.CustomerId);
                    if (wallet == null)
                    {
                        wallet = new Wallet
                        {
                            CustomerId = request.CustomerId,
                            Balance = 0,
                            DateCreated = DateTime.UtcNow
                        };
                        await walletRepository.AddAsync(wallet);
                        await unitOfWork.SaveAsync();
                    }

                    var reference = $"WLT-{Guid.NewGuid().ToString("N")[..12]}";

                    var paystackResponse = await paystackService.InitializeTransactionAsync(customer.Email, request.Amount, reference);

                    if (!paystackResponse.Status) return Result<FundWalletResponse>.Failure("Failed to initialize wallet funding");

                    var transaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = request.Amount,
                        Type = WalletTransactionType.Credit,
                        Status = PaystackStatus.Pending,
                        PaystackReference = reference,
                        Description = "Wallet top-up",
                        DateCreated = DateTime.UtcNow
                    };

                    await walletTransactionRepository.AddAsync(transaction);
                    await unitOfWork.SaveAsync();

                    return Result<FundWalletResponse>.Success(
                        new FundWalletResponse(paystackResponse.AuthorizationUrl, reference),
                        "Wallet funding initialized successfully");
                }
                catch (Exception ex)
                {
                    return Result<FundWalletResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record FundWalletResponse(string AuthorizationUrl, string Reference);
    }
}