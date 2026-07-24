using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetPaymentByOrder
    {
        public record GetPaymentByOrderQuery(Guid OrderId) : IRequest<Result<ICollection<GetPaymentByOrderResponse>>>;

        public class GetPaymentByOrderValidator : AbstractValidator<GetPaymentByOrderQuery>
        {
            public GetPaymentByOrderValidator()
            {
                RuleFor(x => x.OrderId)
                    .NotEmpty()
                    .WithMessage("Order ID is required");
            }
        }

        public class GetPaymentByOrderHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPaymentByOrderQuery, Result<ICollection<GetPaymentByOrderResponse>>>
        {
            public async Task<Result<ICollection<GetPaymentByOrderResponse>>> Handle(GetPaymentByOrderQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var payments = await paymentRepository.GetByOrderIdAsync(request.OrderId);
                    return Result<ICollection<GetPaymentByOrderResponse>>.Success(payments.Adapt<ICollection<GetPaymentByOrderResponse>>(), "Payments retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetPaymentByOrderResponse>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetPaymentByOrderResponse(Guid Id, decimal AmountPaid, decimal AmountTotal, decimal OutstandingBalance, string Status, bool IsConfirmed, DateTime PaymentDate);
    }
}