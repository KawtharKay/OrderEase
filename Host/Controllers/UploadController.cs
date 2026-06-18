using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.UploadFile;

namespace Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController(IMediator mediator) : ControllerBase
    {
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage([FromForm] UploadFileCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }
    }
}
