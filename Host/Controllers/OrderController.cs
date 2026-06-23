using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CreateOrder;
using static Application.Commands.UpdateOrderStatus;
using static Application.Queries.GetAllOrders;
using static Application.Queries.GetOrderById;
using static Application.Queries.GetOrdersByCustomer;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Supplier")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusCommand command)
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

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomer(Guid customerId)
        {
            var response = await mediator.Send(new GetOrdersByCustomerQuery(customerId));
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = "Supplier")]
        public async Task<IActionResult> GetAllOrders()
        {
            var response = await mediator.Send(new GetAllOrdersQuery());
            return Ok(response);
        }
    }
}