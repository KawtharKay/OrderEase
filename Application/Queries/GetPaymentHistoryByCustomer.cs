using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetPaymentHistoryByCustomer
    {
        public record GetPaymentHistoryByCustomerQuery(Guid CustomerId) : IRequest<Result<GetPaymentHistoryByCustomerResponse>>;

        public class GetPaymentHistoryByCustomerValidator : AbstractValidator<GetPaymentHistoryByCustomerQuery>
        {
            public GetPaymentHistoryByCustomerValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer ID is required");
            }
        }

        public class GetPaymentHistoryByCustomerHandler(IPaymentRepository paymentRepository, ICustomerRepository customerRepository) : IRequestHandler<GetPaymentHistoryByCustomerQuery, Result<GetPaymentHistoryByCustomerResponse>>
        {
            public async Task<Result<GetPaymentHistoryByCustomerResponse>> Handle(GetPaymentHistoryByCustomerQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetAsync(request.CustomerId);
                    if (customer == null) return Result<GetPaymentHistoryByCustomerResponse>.Failure("Customer not found");

                    var payments = await paymentRepository.GetAllByCustomerIdAsync(request.CustomerId);

                    var totalOutstanding = payments.Where(x => x.IsConfirmed).Sum(x => x.OutstandingBalance);

                    var paymentList = payments.Adapt<ICollection<PaymentHistoryItem>>();

                    return Result<GetPaymentHistoryByCustomerResponse>.Success(new GetPaymentHistoryByCustomerResponse(totalOutstanding, paymentList), "Payment history retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetPaymentHistoryByCustomerResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record PaymentHistoryItem(Guid Id, Guid OrderId, decimal AmountPaid, decimal AmountTotal, decimal OutstandingBalance, DateTime PaymentDate, string PaystackReference, string Status, bool IsConfirmed);

        public record GetPaymentHistoryByCustomerResponse(decimal TotalOutstandingBalance, ICollection<PaymentHistoryItem> Payments);
    }
}