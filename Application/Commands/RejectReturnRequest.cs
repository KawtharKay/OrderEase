using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class RejectReturnRequest
    {
        public record RejectReturnRequestCommand(Guid ReturnRequestId, string RejectionReason) : IRequest<Result<string>>;

        public class RejectReturnRequestValidator : AbstractValidator<RejectReturnRequestCommand>
        {
            public RejectReturnRequestValidator()
            {
                RuleFor(x => x.ReturnRequestId)
                    .NotEmpty()
                    .WithMessage("Return request ID is required");

                RuleFor(x => x.RejectionReason)
                    .NotEmpty()
                    .WithMessage("Rejection reason is required")
                    .MaximumLength(500)
                    .WithMessage("Rejection reason cannot exceed 500 characters");
            }
        }

        public class RejectReturnRequestHandler(IReturnRequestRepository returnRequestRepository, INotificationService notificationService,
            IUnitOfWork unitOfWork) : IRequestHandler<RejectReturnRequestCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(RejectReturnRequestCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var returnRequest = await returnRequestRepository.GetAsync(request.ReturnRequestId);
                    if (returnRequest is null) return Result<string>.Failure("Return request not found");

                    if (returnRequest.Status != ReturnRequestStatus.Pending) return Result<string>.Failure("This return request has already been processed");

                    returnRequest.Status = ReturnRequestStatus.Rejected;
                    returnRequestRepository.Update(returnRequest);
                    await unitOfWork.SaveAsync();

                    await notificationService.SendNotificationAsync(
                        returnRequest.Customer.UserId,
                        "Return Request Rejected",
                        $"Your return request for order {returnRequest.Order.OrderNumber} was not approved. Reason: {request.RejectionReason}",
                        "ReturnRejected",
                        returnRequest.Id);

                    return Result<string>.Success("Rejected", "Return request rejected");
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}