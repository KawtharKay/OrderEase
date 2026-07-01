using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.FundWallet;
using static Application.Commands.VerifyWalletFunding;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController(IMediator mediator) : ControllerBase
    {
        [HttpPost("fund-wallet")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> FundWallet([FromBody] FundWalletCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("verify-wallet/{reference}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyWallet(string reference)
        {
            var response = await mediator.Send(new VerifyWalletFundingCommand(reference));
            return Ok(response);
        }
    }
}