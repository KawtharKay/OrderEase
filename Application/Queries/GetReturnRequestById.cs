using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Queries
{
    public class GetReturnRequestById
    {
        public record GetReturnRequestByIdQuery(Guid Id) : IRequest<Result<GetReturnRequestByIdResponse>>;

        public class GetReturnRequestByIdValidator : AbstractValidator<GetReturnRequestByIdQuery>
        {
            public GetReturnRequestByIdValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Return request ID is required");
            }
        }

        public class GetReturnRequestByIdHandler(IReturnRequestRepository returnRequestRepository) : IRequestHandler<GetReturnRequestByIdQuery, Result<GetReturnRequestByIdResponse>>
        {
            public async Task<Result<GetReturnRequestByIdResponse>> Handle(GetReturnRequestByIdQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var returnRequest = await returnRequestRepository.GetAsync(request.Id);
                    if (returnRequest is null) return Result<GetReturnRequestByIdResponse>.Failure("Return request not found");

                    var response = new GetReturnRequestByIdResponse(
                        returnRequest.Id,
                        returnRequest.Order.OrderNumber,
                        returnRequest.Category.Name,
                        returnRequest.Reason,
                        returnRequest.Status.ToString(),
                        returnRequest.RefundAmount,
                        returnRequest.WalletCreditAmount,
                        returnRequest.DebtReductionAmount,
                        returnRequest.RejectionReason,
                        returnRequest.ReturnRequestItems.Select(x => new ReturnRequestItemResponse(
                            x.ItemId,
                            x.Item.Title,
                            x.Quantity,
                            x.UnitPrice,
                            x.Quantity * x.UnitPrice)).ToList());

                    return Result<GetReturnRequestByIdResponse>.Success(
                        response, "Return request retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetReturnRequestByIdResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record ReturnRequestItemResponse(Guid ItemId, string Title, int Quantity, decimal UnitPrice, decimal SubTotal);

        public record GetReturnRequestByIdResponse(Guid Id, string OrderNumber, string CategoryName, string Reason, string Status,
            decimal RefundAmount, decimal WalletCreditAmount, decimal DebtReductionAmount, string? RejectionReason,
            ICollection<ReturnRequestItemResponse> ReturnRequestItems);
    }
}
