using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.ApproveReturnRequest;
using static Application.Commands.CreateReturnRequest;
using static Application.Commands.RejectReturnRequest;
using static Application.Queries.GetAllReturnRequests;
using static Application.Queries.GetReturnRequestById;
using static Application.Queries.GetReturnRequestsByCustomer;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReturnRequestsController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateReturnRequest([FromBody] CreateReturnRequestCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Supplier")]
        public async Task<IActionResult> ApproveReturnRequest(Guid id)
        {
            var response = await mediator.Send(new ApproveReturnRequestCommand(id));
            return Ok(response);
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Supplier")]
        public async Task<IActionResult> RejectReturnRequest(Guid id, [FromBody] RejectReturnRequestCommand command)
        {
            var response = await mediator.Send(command with { ReturnRequestId = id });
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await mediator.Send(new GetReturnRequestByIdQuery(id));
            return Ok(response);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(Guid customerId)
        {
            var response = await mediator.Send(new GetReturnRequestsByCustomerQuery(customerId));
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = "Supplier")]
        public async Task<IActionResult> GetAll()
        {
            var response = await mediator.Send(new GetAllReturnRequestsQuery());
            return Ok(response);
        }
    }
}