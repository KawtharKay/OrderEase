using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetAllReturnRequests
    {
        public record GetAllReturnRequestsQuery() : IRequest<Result<ICollection<GetAllReturnRequestsResponse>>>;

        public class GetAllReturnRequestsHandler(IReturnRequestRepository returnRequestRepository)
            : IRequestHandler<GetAllReturnRequestsQuery, Result<ICollection<GetAllReturnRequestsResponse>>>
        {
            public async Task<Result<ICollection<GetAllReturnRequestsResponse>>> Handle(GetAllReturnRequestsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var returnRequests = await returnRequestRepository.GetAllAsync();
                    var response = returnRequests.Select(x => new GetAllReturnRequestsResponse(
                        x.Id,
                        x.Customer.Name,
                        x.Order.OrderNumber,
                        x.Category.Name,
                        x.Reason,
                        x.Status.ToString(),
                        x.DateCreated)).ToList();

                    return Result<ICollection<GetAllReturnRequestsResponse>>.Success(response, "Return requests retrieved successfully");
                }
                catch (Exception ex)
                {
                    return Result<ICollection<GetAllReturnRequestsResponse>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetAllReturnRequestsResponse(Guid Id, string CustomerName, string OrderNumber, string CategoryName, string Reason, string Status, DateTime DateCreated);
    }
}