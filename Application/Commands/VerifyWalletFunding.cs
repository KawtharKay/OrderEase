using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands
{
    public class VerifyWalletFunding
    {
        public record VerifyWalletFundingCommand(string Reference) : IRequest<Result<string>>;

        public class VerifyWalletFundingValidator : AbstractValidator<VerifyWalletFundingCommand>
        {
            public VerifyWalletFundingValidator()
            {
                RuleFor(x => x.Reference)
                    .NotEmpty()
                    .WithMessage("Reference is required");
            }
        }

        public class VerifyWalletFundingHandler(
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IPaystackService paystackService,
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            ILogger<VerifyWalletFundingHandler> logger) : IRequestHandler<VerifyWalletFundingCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                VerifyWalletFundingCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var transaction = await walletTransactionRepository.GetByReferenceAsync(request.Reference);
                    if (transaction == null)
                        return Result<string>.Failure("Wallet transaction not found");

                    if (transaction.Status == PaystackStatus.Successful)
                        return Result<string>.Success("Already confirmed", "This top-up was already verified");

                    var verification = await paystackService.VerifyTransactionAsync(request.Reference);

                    if (verification.ReferenceStatus != "success")
                    {
                        transaction.Status = PaystackStatus.Failed;
                        walletTransactionRepository.Update(transaction);
                        await unitOfWork.SaveAsync();

                        return Result<string>.Failure("Wallet funding was not successful");
                    }

                    transaction.Status = PaystackStatus.Successful;
                    walletTransactionRepository.Update(transaction);

                    var wallet = await walletRepository.GetAsync(transaction.WalletId);
                    if (wallet != null)
                    {
                        wallet.Balance += transaction.Amount;
                        walletRepository.Update(wallet);

                        await notificationService.SendNotificationAsync(
                            wallet.Customer.UserId,
                            "Wallet Funded",
                            $"₦{transaction.Amount:N2} has been added to your wallet. New balance: ₦{wallet.Balance:N2}",
                            "WalletFunded",
                            wallet.Id);
                    }

                    await unitOfWork.SaveAsync();

                    return Result<string>.Success("Verified", "Wallet funded successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error verifying wallet funding for reference {Reference}", request.Reference);
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}