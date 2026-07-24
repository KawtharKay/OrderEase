using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands
{
    public class VerifyPayment
    {
        public record VerifyPaymentCommand(string Reference) : IRequest<Result<string>>;

        public class VerifyPaymentValidator : AbstractValidator<VerifyPaymentCommand>
        {
            public VerifyPaymentValidator()
            {
                RuleFor(x => x.Reference)
                    .NotEmpty()
                    .WithMessage("Payment reference is required");
            }
        }

        public class VerifyPaymentHandler(IPaymentRepository paymentRepository, IOrderRepository orderRepository, ISupplierRepository supplierRepository, IPaystackService paystackService,
           IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository, INotificationService notificationService, IEmailService emailService, IUnitOfWork unitOfWork, ILogger<VerifyPaymentHandler> logger) : IRequestHandler<VerifyPaymentCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var payment = await paymentRepository.GetByPaystackReferenceAsync(request.Reference);
                    if (payment == null) return Result<string>.Failure("Payment record not found");

                    if (payment.IsConfirmed) return Result<string>.Success("Already confirmed", "Payment was already verified");

                    var verification = await paystackService.VerifyTransactionAsync(request.Reference);

                    if (verification.ReferenceStatus != "success")
                    {
                        payment.Status = PaystackStatus.Failed;
                        paymentRepository.Update(payment);
                        await unitOfWork.SaveAsync();

                        return Result<string>.Failure("Payment was not successful");
                    }

                    var order = await orderRepository.GetAsync(payment.OrderId);

                    var overpaidAmount = verification.AmountPaid - payment.AmountTotal;

                    if (overpaidAmount > 0)
                    {
                        payment.AmountPaid = payment.AmountTotal;
                        payment.OutstandingBalance = 0;

                        var wallet = await walletRepository.GetByCustomerAsync(payment.CustomerId);
                        if (wallet == null)
                        {
                            wallet = new Wallet
                            {
                                CustomerId = payment.CustomerId,
                                Balance = 0,
                                DateCreated = DateTime.UtcNow
                            };
                            await walletRepository.AddAsync(wallet);
                            await unitOfWork.SaveAsync();
                        }

                        wallet.Balance += overpaidAmount;
                        walletRepository.Update(wallet);

                        var walletTransaction = new WalletTransaction
                        {
                            WalletId = wallet.Id,
                            Amount = overpaidAmount,
                            Type = WalletTransactionType.Credit,
                            Status = PaystackStatus.Successful,
                            Description = $"Overpayment from order {order?.OrderNumber}",
                            DateCreated = DateTime.UtcNow
                        };
                        await walletTransactionRepository.AddAsync(walletTransaction);
                    }
                    else
                    {
                        payment.AmountPaid = verification.AmountPaid;
                        payment.OutstandingBalance = payment.AmountTotal - verification.AmountPaid;
                    }

                    payment.Status = PaystackStatus.Successful;
                    payment.IsConfirmed = true;
                    payment.DateConfirmed = DateTime.UtcNow;
                    paymentRepository.Update(payment);
                    await unitOfWork.SaveAsync();

                    var supplier = await supplierRepository.GetFirstAsync();
                    if (supplier != null && order != null)
                    {
                        await notificationService.SendNotificationAsync(
                            supplier.UserId,
                            "Payment Received",
                            $"Payment of ₦{payment.AmountPaid:N2} received for order {order.OrderNumber}." +
                            (overpaidAmount > 0 ? $" ₦{overpaidAmount:N2} was credited to the customer's wallet." : ""),
                            "PaymentConfirmed",
                            payment.OrderId);

                        try
                        {
                            await emailService.SendGenericEmailAsync(
                                supplier.Email,
                                "Payment Received on OrderEase",
                                $"<h2>Payment Confirmed</h2><p>₦{payment.AmountPaid:N2} received for order {order.OrderNumber}.</p>");
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to send payment confirmation email to supplier");
                        }
                    }

                    if (order != null)
                    {
                        await notificationService.SendNotificationAsync(
                            order.Customer.UserId,
                            "Payment Confirmed",
                            overpaidAmount > 0
                                ? $"Your payment was confirmed. ₦{overpaidAmount:N2} extra was added to your wallet."
                                : $"Your payment of ₦{payment.AmountPaid:N2} for order {order.OrderNumber} has been confirmed",
                            "PaymentConfirmed",
                            payment.OrderId);
                    }

                    return Result<string>.Success("Verified", "Payment verified and confirmed successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error verifying payment for reference {Reference}", request.Reference);
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}