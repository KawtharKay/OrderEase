using Application.Common.Dtos;
using FluentValidation;
using MediatR;

namespace Application.Queries
{
    public class GetWalletBalance
    {
        public record GetWalletBalanceQuery(Guid CustomerId) : IRequest<Result<GetWalletBalanceResponse>>;

        public class GetWalletBalanceValidator : AbstractValidator<GetWalletBalanceQuery>
        {
            public GetWalletBalanceValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer ID is required");
            }
        }

        public class GetWalletBalanceHandler(IWalletRepository walletRepository) : IRequestHandler<GetWalletBalanceQuery, Result<GetWalletBalanceResponse>>
        {
            public async Task<Result<GetWalletBalanceResponse>> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var wallet = await walletRepository.GetByCustomerAsync(request.CustomerId);

                    var balance = wallet?.Balance ?? 0;

                    return Result<GetWalletBalanceResponse>.Success(new GetWalletBalanceResponse(request.CustomerId, balance), "Wallet balance retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetWalletBalanceResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetWalletBalanceResponse(Guid CustomerId, decimal Balance);
    }
}