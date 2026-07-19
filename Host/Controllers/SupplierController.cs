using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Queries.GetSupplierProfile;

namespace Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Supplier)]
    public class SupplierController(IMediator mediator) : ControllerBase
    {
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var response = await mediator.Send(new GetSupplierProfileQuery());
            return Ok(response);
        }
    }
}