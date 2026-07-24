using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Queries.GetCustomerProfile;

namespace Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Customer)]
    public class CustomerController(IMediator mediator) : ControllerBase
    {
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var response = await mediator.Send(new GetCustomerProfileQuery());
            return Ok(response);
        }
    }
}