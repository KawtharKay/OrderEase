using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CreateDelivery;
using static Application.Commands.UpdateDelivery;
using static Application.Queries.GetDeliveryByOrder;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveryController(IMediator mediator) : ControllerBase
    {
        [HttpPost("create-delivery")]
        [Authorize(Roles = "Supplier")]
        public async Task<IActionResult> CreateDelivery([FromBody] CreateDeliveryCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPatch("{orderId}")]
        [Authorize(Roles = "Supplier")]
        public async Task<IActionResult> UpdateDelivery(Guid orderId, [FromBody] UpdateDeliveryCommand command)
        {
            var response = await mediator.Send(command with { OrderId = orderId });
            return Ok(response);
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetDeliveryByOrder(Guid orderId)
        {
            var response = await mediator.Send(new GetDeliveryByOrderQuery(orderId));
            return Ok(response);
        }
    }
}