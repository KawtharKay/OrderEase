using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CreateUser;

namespace Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }
    }
}
