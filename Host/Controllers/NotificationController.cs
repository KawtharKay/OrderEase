using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.MarkNotificationsAsRead;
using static Application.Queries.GetMyNotifications;

namespace Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController(IMediator mediator) : ControllerBase
    {
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var response = await mediator.Send(new GetMyNotificationsQuery());
            return Ok(response);
        }

        [HttpPatch("mark-read")]
        public async Task<IActionResult> MarkAsRead()
        {
            var response = await mediator.Send(new MarkNotificationsAsReadCommand());
            return Ok(response);
        }
    }
}