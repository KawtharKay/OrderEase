using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Commands
{
    public class ApproveReturnRequest
    {
        public record ApproveReturnRequestCommand(Guid ReturnRequestId) : IRequest<Result<string>>;

        public class ApproveReturnRequestValidator : AbstractValidator<ApproveReturnRequestCommand>
        {
            public ApproveReturnRequestValidator()
            {
                RuleFor(x => x.ReturnRequestId)
                    .NotEmpty()
                    .WithMessage("Return request ID is required");
            }
        }

        public class ApproveReturnRequestHandler(IReturnRequestRepository returnRequestRepository, IItemRepository itemRepository, IPaymentRepository paymentRepository,
            IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository, INotificationService notificationService,
            IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<ApproveReturnRequestHandler> logger)
            : IRequestHandler<ApproveReturnRequestCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(ApproveReturnRequestCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var returnRequest = await returnRequestRepository.GetAsync(request.ReturnRequestId);
                    if (returnRequest is null) return Result<string>.Failure("Return request not found");

                    if (returnRequest.Status != ReturnRequestStatus.Pending) return Result<string>.Failure("This return request has already been processed");

                    decimal totalRefundAmount = 0;

                    foreach (var returnItem in returnRequest.ReturnRequestItems)
                    {
                        var item = await itemRepository.GetAsync(returnItem.ItemId);
                        if (item != null)
                        {
                            item.Quantity += returnItem.Quantity;
                            if (item.Quantity > 0 && !item.IsAvailable)
                                item.IsAvailable = true;

                            totalRefundAmount += returnItem.Quantity * item.Price;

                            itemRepository.Update(item);
                        }
                    }

                    returnRequest.Status = ReturnRequestStatus.Approved;
                    returnRequest.RefundAmount = totalRefundAmount;
                    returnRequestRepository.Update(returnRequest);

                    var confirmedPayments = await paymentRepository.GetByOrderIdAsync(returnRequest.OrderId);
                    var totalPaid = confirmedPayments
                        .Where(x => x.IsConfirmed)
                        .Sum(x => x.AmountPaid);

                    var order = await orderRepository.GetAsync(returnRequest.OrderId);
                    var walletUsed = order?.WalletAmountUsed ?? 0;
                    var totalEffectivelyPaid = totalPaid + walletUsed;

                    string notificationMessage;

                    if (totalEffectivelyPaid <= 0)
                    {
                        if (order != null)
                        {
                            order.AmountOwed = Math.Max(0, order.AmountOwed - totalRefundAmount);
                            orderRepository.Update(order);
                        }

                        returnRequest.DebtReductionAmount = totalRefundAmount;

                        notificationMessage = $"Your return request for order {returnRequest.Order.OrderNumber} was approved. " +
                            $"₦{totalRefundAmount:N2} has been deducted from your outstanding balance.";
                    }

                    else if (totalEffectivelyPaid >= returnRequest.Order.TotalPrice)
                    {
                        await CreditWalletAsync(
                            returnRequest.CustomerId,
                            totalRefundAmount,
                            $"Refund for approved return on order {returnRequest.Order.OrderNumber}");

                        returnRequest.WalletCreditAmount = totalRefundAmount;

                        notificationMessage = $"Your return request for order {returnRequest.Order.OrderNumber} was approved. " +
                            $"₦{totalRefundAmount:N2} has been credited to your wallet.";
                    }

                    else
                    {
                        var orderTotal = returnRequest.Order.TotalPrice;
                        var unpaidAmount = orderTotal - totalEffectivelyPaid;
                        var refundProportion = totalRefundAmount / orderTotal;

                        var walletCreditAmount = Math.Round(totalRefundAmount - (refundProportion * unpaidAmount), 2);
                        var debtReductionAmount = totalRefundAmount - walletCreditAmount;

                        if (walletCreditAmount > 0)
                        {
                            await CreditWalletAsync(
                                returnRequest.CustomerId,
                                walletCreditAmount,
                                $"Partial refund for approved return on order {returnRequest.Order.OrderNumber}");
                        }

                        if (debtReductionAmount > 0 && order != null)
                        {
                            order.AmountOwed = Math.Max(0, order.AmountOwed - debtReductionAmount);
                            orderRepository.Update(order);
                        }

                        returnRequest.WalletCreditAmount = walletCreditAmount;
                        returnRequest.DebtReductionAmount = debtReductionAmount;

                        notificationMessage = $"Your return request for order {returnRequest.Order.OrderNumber} was approved. " +
                            $"₦{walletCreditAmount:N2} credited to your wallet and ₦{debtReductionAmount:N2} deducted from your outstanding balance.";
                    }

                    await unitOfWork.SaveAsync();

                    await notificationService.SendNotificationAsync(returnRequest.Customer.UserId, "Return Request Approved", $"Your return request for order {returnRequest.Order.OrderNumber} has been approved", "ReturnApproved", returnRequest.Id);

                    return Result<string>.Success("Approved", "Return request approved and stock updated");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Result<string>.Failure("Item stock was just updated elsewhere. Please try again.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error approving return request {ReturnRequestId}", request.ReturnRequestId);
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }

            private async Task CreditWalletAsync(Guid customerId, decimal amount, string description)
            {
                var wallet = await walletRepository.GetByCustomerAsync(customerId);
                if (wallet == null)
                {
                    wallet = new Wallet
                    {
                        CustomerId = customerId,
                        Balance = 0,
                        DateCreated = DateTime.UtcNow
                    };
                    await walletRepository.AddAsync(wallet);
                }

                wallet.Balance += amount;
                walletRepository.Update(wallet);

                var walletTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = amount,
                    Type = WalletTransactionType.Credit,
                    Status = PaystackStatus.Successful,
                    Description = description,
                    DateCreated = DateTime.UtcNow
                };
                await walletTransactionRepository.AddAsync(walletTransaction);
            }
        }
    }
}