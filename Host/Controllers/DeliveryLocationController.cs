using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CreateDeliveryLocation;
using static Application.Commands.UpdateDeliveryLocation;
using static Application.Queries.GetDeliveryLocations;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveryLocationController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetActiveLocations()
        {
            var response = await mediator.Send(new GetDeliveryLocationsQuery(true));
            return Ok(response);
        }

        [HttpGet("supplier")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> GetAllLocationsForSupplier()
        {
            var response = await mediator.Send(new GetDeliveryLocationsQuery(false));
            return Ok(response);
        }

        [HttpPost("create")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> CreateDeliveryLocation([FromBody] CreateDeliveryLocationCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> UpdateDeliveryLocation(Guid id, [FromBody] UpdateDeliveryLocationCommand command)
        {
            var response = await mediator.Send(command with { Id = id });
            return Ok(response);
        }
    }
}
