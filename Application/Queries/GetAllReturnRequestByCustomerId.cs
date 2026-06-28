using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetReturnRequestsByCustomer
    {
        public record GetReturnRequestsByCustomerQuery(Guid CustomerId) : IRequest<Result<ICollection<GetReturnRequestsByCustomerResponse>>>;

        public class GetReturnRequestsByCustomerValidator : AbstractValidator<GetReturnRequestsByCustomerQuery>
        {
            public GetReturnRequestsByCustomerValidator()
            {
                RuleFor(x => x.CustomerId)
                    .NotEmpty()
                    .WithMessage("Customer ID is required");
            }
        }

        public class GetReturnRequestsByCustomerHandler(IReturnRequestRepository returnRequestRepository)
            : IRequestHandler<GetReturnRequestsByCustomerQuery, Result<ICollection<GetReturnRequestsByCustomerResponse>>>
        {
            public async Task<Result<ICollection<GetReturnRequestsByCustomerResponse>>> Handle(GetReturnRequestsByCustomerQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var returnRequests = await returnRequestRepository.GetAllByCustomerIdAsync(request.CustomerId);
                    return Result<ICollection<GetReturnRequestsByCustomerResponse>>.Success(returnRequests.Adapt<ICollection<GetReturnRequestsByCustomerResponse>>(), "Return requests retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetReturnRequestsByCustomerResponse>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetReturnRequestsByCustomerResponse(Guid Id, Guid OrderId, string Reason, string Status, DateTime DateCreated);
    }
}