using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class PayOrderWithWallet
    {
        public record PayOrderWithWalletCommand(Guid OrderId, Guid CustomerId) : IRequest<Result<PayOrderWithWalletResponse>>;

        public class PayOrderWithWalletValidator : AbstractValidator<PayOrderWithWalletCommand>
        {
            public PayOrderWithWalletValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order is required");
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer is required");
            }
        }

        public class PayOrderWithWalletHandler(IOrderRepository orderRepository, IPaymentRepository paymentRepository,
            IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository, IUnitOfWork unitOfWork)
            : IRequestHandler<PayOrderWithWalletCommand, Result<PayOrderWithWalletResponse>>
        {
            public async Task<Result<PayOrderWithWalletResponse>> Handle(PayOrderWithWalletCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetAsync(request.OrderId);
                    if (order is null) return Result<PayOrderWithWalletResponse>.Failure("Order not found");

                    if (order.CustomerId != request.CustomerId) return Result<PayOrderWithWalletResponse>.Failure("This order does not belong to this customer");

                    var confirmedPayments = await paymentRepository.GetByOrderIdAsync(order.Id);
                    var paystackPaid = confirmedPayments.Where(x => x.IsConfirmed).Sum(x => x.AmountPaid);
                    var totalPaid = order.WalletAmountUsed + paystackPaid;
                    var outstanding = Math.Max(0, order.TotalPrice - totalPaid);

                    if (outstanding <= 0) return Result<PayOrderWithWalletResponse>.Failure("This order has no outstanding balance");

                    var wallet = await walletRepository.GetByCustomerAsync(request.CustomerId);
                    if (wallet is null || wallet.Balance <= 0) return Result<PayOrderWithWalletResponse>.Failure("Your wallet has no funds available");

                    var amountToApply = Math.Min(wallet.Balance, outstanding);

                    wallet.Balance -= amountToApply;
                    walletRepository.Update(wallet);

                    order.WalletAmountUsed += amountToApply;
                    orderRepository.Update(order);

                    await walletTransactionRepository.AddAsync(new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = amountToApply,
                        Type = WalletTransactionType.Debit,
                        Status = PaystackStatus.Successful,
                        Description = $"Applied to outstanding balance on order {order.OrderNumber}",
                        DateCreated = DateTime.UtcNow
                    });

                    await unitOfWork.SaveAsync();

                    var remainingOutstanding = Math.Max(0, outstanding - amountToApply);

                    return Result<PayOrderWithWalletResponse>.Success(
                        new PayOrderWithWalletResponse(amountToApply, remainingOutstanding, wallet.Balance),
                        remainingOutstanding > 0
                            ? $"₦{amountToApply:N2} applied from your wallet. ₦{remainingOutstanding:N2} still outstanding."
                            : $"₦{amountToApply:N2} applied from your wallet. Order fully paid.");
                }
                catch (Exception ex)
                {
                    return Result<PayOrderWithWalletResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record PayOrderWithWalletResponse(decimal AmountApplied, decimal RemainingOutstanding, decimal NewWalletBalance);
    }
}