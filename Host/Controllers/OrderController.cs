using Application.Constants;
using Application.Repositories;
using Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CancelOrder;
using static Application.Commands.CreateOrder;
using static Application.Commands.PayOrderWithWallet;
using static Application.Commands.SetDeliveryCharges;
using static Application.Commands.UpdateOrderStatus;
using static Application.Queries.GetAllOrders;
using static Application.Queries.GetOrderById;
using static Application.Queries.GetOrdersByCustomer;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController(IMediator mediator, ICurrentUser currentUser, ICustomerRepository customerRepository) : ControllerBase
    {
        private async Task<Guid?> ResolveCustomerIdAsync()
        {
            var customer = await customerRepository.GetByUserIdAsync(currentUser.GetCurrentUserId());
            return customer?.Id;
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var customerId = await ResolveCustomerIdAsync();
            if (customerId is null) return BadRequest("Customer profile not found for this account");

            var response = await mediator.Send(command with { CustomerId = customerId.Value });
            return Ok(response);
        }

        [HttpPost("{id}/pay-with-wallet")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> PayWithWallet(Guid id)
        {
            var customerId = await ResolveCustomerIdAsync();
            if (customerId is null) return BadRequest("Customer profile not found for this account");

            var response = await mediator.Send(new PayOrderWithWalletCommand(id, customerId.Value));
            return Ok(response);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusCommand command)
        {
            var response = await mediator.Send(command with { OrderId = id });
            return Ok(response);
        }

        [HttpPut("{id}/delivery-charges")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> SetDeliveryCharges(Guid id, [FromBody] SetDeliveryChargesCommand command)
        {
            var response = await mediator.Send(command with { OrderId = id });
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var response = await mediator.Send(new GetOrderByIdQuery(id));
            return Ok(response);
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetMyOrders()
        {
            var customerId = await ResolveCustomerIdAsync();
            if (customerId is null) return BadRequest("Customer profile not found for this account");

            var response = await mediator.Send(new GetOrdersByCustomerQuery(customerId.Value));
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> GetAllOrders()
        {
            var response = await mediator.Send(new GetAllOrdersQuery());
            return Ok(response);
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var customerId = await ResolveCustomerIdAsync();
            if (customerId is null) return BadRequest("Customer profile not found for this account");

            var response = await mediator.Send(new CancelOrderCommand(id, customerId.Value));
            return Ok(response);
        }
    }
}