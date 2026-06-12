using Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CreateUser;
using static Application.Commands.LoginUser;
using static Application.Commands.RegisterCustomer;
using static Application.Commands.RegisterSupplier;
using static Application.Commands.ResendVerificationEmail;
using static Application.Commands.VerifyEmail;

namespace Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("register-supplier")]
        public async Task<IActionResult> RegisterSupplier([FromBody] RegisterSupplierCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var response = await mediator.Send(new VerifyEmailCommand(token));
            return Ok(response);
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationEmailCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }
    }
}
