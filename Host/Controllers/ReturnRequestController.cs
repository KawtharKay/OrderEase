using Application.Constants;
using Application.Repositories;
using Application.Services;
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
    public class ReturnRequestsController(IMediator mediator, ICurrentUser currentUser, ICustomerRepository customerRepository) : ControllerBase
    {
        private async Task<Guid?> ResolveCustomerIdAsync()
        {
            var customer = await customerRepository.GetByUserIdAsync(currentUser.GetCurrentUserId());
            return customer?.Id;
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> CreateReturnRequest([FromBody] CreateReturnRequestCommand command)
        {
            var customerId = await ResolveCustomerIdAsync();
            if (customerId is null) return BadRequest("Customer profile not found for this account");

            var response = await mediator.Send(command with { CustomerId = customerId.Value });
            return Ok(response);
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> ApproveReturnRequest(Guid id)
        {
            var response = await mediator.Send(new ApproveReturnRequestCommand(id));
            return Ok(response);
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Roles = AppRoles.Supplier)]
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

        [HttpGet("my-requests")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetMyRequests()
        {
            var customerId = await ResolveCustomerIdAsync();
            if (customerId is null) return BadRequest("Customer profile not found for this account");

            var response = await mediator.Send(new GetReturnRequestsByCustomerQuery(customerId.Value));
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> GetAll()
        {
            var response = await mediator.Send(new GetAllReturnRequestsQuery());
            return Ok(response);
        }
    }
}