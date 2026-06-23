using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
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
            INotificationService notificationService, IEmailService emailService, IUnitOfWork unitOfWork, ILogger<VerifyPaymentHandler> logger) : IRequestHandler<VerifyPaymentCommand, Result<string>>
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

                    payment.AmountPaid = verification.AmountPaid;
                    payment.OutstandingBalance = payment.AmountTotal - verification.AmountPaid;
                    payment.Status = PaystackStatus.Successful;
                    payment.IsConfirmed = true;
                    payment.DateConfirmed = DateTime.UtcNow;
                    paymentRepository.Update(payment);
                    await unitOfWork.SaveAsync();

                    var order = await orderRepository.GetAsync(payment.OrderId);

                    var supplier = await supplierRepository.GetFirstAsync();
                    if (supplier != null && order != null)
                    {
                        await notificationService.SendNotificationAsync(supplier.UserId, "Payment Received", $"Payment of ₦{payment.AmountPaid:N2} received for order {order.OrderNumber}." +
                            $" Outstanding balance: ₦{payment.OutstandingBalance:N2}", "PaymentConfirmed", payment.OrderId);

                        try
                        {
                            await emailService.SendGenericEmailAsync(supplier.Email, "Payment Received on OrderEase",
                                $"<h2>Payment Confirmed</h2><p>₦{payment.AmountPaid:N2} received for order {order.OrderNumber}.</p><p>Outstanding balance: ₦{payment.OutstandingBalance:N2}</p>");
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to send payment confirmation email to supplier");
                        }
                    }

                    if (order != null)
                    {
                        await notificationService.SendNotificationAsync(order.Customer.UserId, "Payment Confirmed", $"Your payment of ₦{payment.AmountPaid:N2} for order {order.OrderNumber} " +
                            $"has been confirmed", "PaymentConfirmed", payment.OrderId);
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