using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class InitiatePayment
    {
        public record InitiatePaymentCommand(Guid OrderId, Guid CustomerId) : IRequest<Result<InitiatePaymentResponse>>;

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
            }
        }

        public class InitiatePaymentHandler(IOrderRepository orderRepository, ICustomerRepository customerRepository, IPaymentRepository paymentRepository, IPaystackService paystackService,
            IUnitOfWork unitOfWork) : IRequestHandler<InitiatePaymentCommand, Result<InitiatePaymentResponse>>
        {
            public async Task<Result<InitiatePaymentResponse>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetAsync(request.OrderId);
                    if (order == null) return Result<InitiatePaymentResponse>.Failure("Order not found");

                    var customer = await customerRepository.GetAsync(request.CustomerId);
                    if (customer == null) return Result<InitiatePaymentResponse>.Failure("Customer not found");

                    var reference = $"PSK-{Guid.NewGuid().ToString("N")[..12]}";

                    var paystackResponse = await paystackService.InitializeTransactionAsync(customer.Email, order.TotalPrice, reference);

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