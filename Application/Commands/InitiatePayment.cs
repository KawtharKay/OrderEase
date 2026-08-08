using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Commands
{
    public class InitiatePayment
    {
        public record InitiatePaymentCommand(Guid OrderId, Guid CustomerId, decimal? Amount) : IRequest<Result<InitiatePaymentResponse>>;

        public class InitiatePaymentValidator : AbstractValidator<InitiatePaymentCommand>
        {
            public InitiatePaymentValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order ID is required");

                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer ID is required");

                RuleFor(x => x.Amount)
                    .GreaterThan(0)
                    .When(x => x.Amount.HasValue)
                    .WithMessage("Amount must be greater than zero");
            }
        }

        public class InitiatePaymentHandler(IOrderRepository orderRepository, ICustomerRepository customerRepository, IPaymentRepository paymentRepository, IPaystackService paystackService,
            IConfiguration configuration, IUnitOfWork unitOfWork) : IRequestHandler<InitiatePaymentCommand, Result<InitiatePaymentResponse>>
        {
            public async Task<Result<InitiatePaymentResponse>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetAsync(request.OrderId);
                    if (order == null) return Result<InitiatePaymentResponse>.Failure("Order not found");

                    var customer = await customerRepository.GetAsync(request.CustomerId);
                    if (customer == null) return Result<InitiatePaymentResponse>.Failure("Customer not found");

                    var confirmedPayments = await paymentRepository.GetByOrderIdAsync(order.Id);
                    var paystackPaid = confirmedPayments.Where(x => x.IsConfirmed).Sum(x => x.AmountPaid);
                    var totalPaid = order.WalletAmountUsed + paystackPaid;
                    var outstanding = Math.Max(0, order.TotalPrice - totalPaid);

                    if (outstanding <= 0) return Result<InitiatePaymentResponse>.Failure("This order has already been fully paid");

                    var amountToPay = request.Amount ?? outstanding;
                    if (amountToPay > outstanding) return Result<InitiatePaymentResponse>.Failure($"You can pay at most ₦{outstanding:N2} on this order");

                    var reference = $"ORDP-{Guid.NewGuid().ToString("N")[..12]}";
                    var baseUrl = configuration["AppSettings:BaseUrl"];
                    var callbackUrl = $"{baseUrl}/paymentCallback.html";

                    var paystackResponse = await paystackService.InitializeTransactionAsync(customer.Email, amountToPay, reference, callbackUrl);
                    if (!paystackResponse.Status) return Result<InitiatePaymentResponse>.Failure("Failed to initialize payment");

                    var payment = new Payment
                    {
                        CustomerId = request.CustomerId,
                        OrderId = request.OrderId,
                        AmountPaid = 0,
                        AmountTotal = order.TotalPrice,
                        OutstandingBalance = order.TotalPrice,
                        PaymentDate = DateTime.UtcNow,
                        PaystackReference = reference,
                        Status = PaystackStatus.Pending,
                        IsConfirmed = false,
                        DateCreated = DateTime.UtcNow
                    };

                    await paymentRepository.AddAsync(payment);
                    await unitOfWork.SaveAsync();

                    return Result<InitiatePaymentResponse>.Success(new InitiatePaymentResponse(paystackResponse.AuthorizationUrl, reference), "Payment initialized successfully");
                }
                catch (Exception ex)
                {
                    return Result<InitiatePaymentResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record InitiatePaymentResponse(string AuthorizationUrl, string Reference);
    }
}