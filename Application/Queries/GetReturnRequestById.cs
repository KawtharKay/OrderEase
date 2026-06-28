using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using Mapster;
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

                    return Result<GetReturnRequestByIdResponse>.Success(returnRequest.Adapt<GetReturnRequestByIdResponse>(), "Return request retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<GetReturnRequestByIdResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record ReturnRequestItemResponse(Guid ItemId, string Title, int Quantity);

        public record GetReturnRequestByIdResponse(Guid Id, Guid OrderId, Guid CategoryId, string Reason, string Status, List<ReturnRequestItemResponse> ReturnRequestItems);
    }
}