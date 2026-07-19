using Application.Common.Dtos;
using FluentValidation;
using MediatR;

namespace Application.Queries
{
    public class GetWalletTransactionHistory
    {
        public record GetWalletTransactionHistoryQuery(Guid CustomerId) : IRequest<Result<GetWalletTransactionHistoryResponse>>;

        public class GetWalletTransactionHistoryValidator : AbstractValidator<GetWalletTransactionHistoryQuery>
        {
            public GetWalletTransactionHistoryValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer ID is required");
            }
        }

        public class GetWalletTransactionHistoryHandler(IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository)
            : IRequestHandler<GetWalletTransactionHistoryQuery, Result<GetWalletTransactionHistoryResponse>>
        {
            public async Task<Result<GetWalletTransactionHistoryResponse>> Handle(GetWalletTransactionHistoryQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var wallet = await walletRepository.GetByCustomerAsync(request.CustomerId);
                    if (wallet == null)
                        return Result<GetWalletTransactionHistoryResponse>.Success(
                            new GetWalletTransactionHistoryResponse(0, new List<WalletTransactionItem>()),
                            "No wallet found — balance is zero");

                    var transactions = await walletTransactionRepository.GetByWalletAsync(wallet.Id);

                    var transactionList = transactions.Select(x => new WalletTransactionItem(
                        x.Id,
                        x.Amount,
                        x.Type.ToString(),
                        x.Status.ToString(),
                        x.Description,
                        x.DateCreated)).ToList();

                    return Result<GetWalletTransactionHistoryResponse>.Success(
                        new GetWalletTransactionHistoryResponse(wallet.Balance, transactionList),
                        "Wallet transaction history retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetWalletTransactionHistoryResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record WalletTransactionItem(Guid Id, decimal Amount, string Type, string Status, string Description, DateTime DateCreated);

        public record GetWalletTransactionHistoryResponse(decimal CurrentBalance, List<WalletTransactionItem> Transactions);
    }
}