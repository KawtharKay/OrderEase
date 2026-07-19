using Application.Constants;
using Application.Repositories;
using Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.FundWallet;
using static Application.Commands.VerifyWalletFunding;
using static Application.Queries.GetWalletBalance;
using static Application.Queries.GetWalletTransactionHistory;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController(IMediator mediator, ICurrentUser currentUser, ICustomerRepository customerRepository) : ControllerBase
    {
        private async Task<Guid?> ResolveCustomerIdAsync()
        {
            var customer = await customerRepository.GetByUserIdAsync(currentUser.GetCurrentUserId());
            return customer?.Id;
        }

        [HttpPost("fund-wallet")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> FundWallet([FromBody] FundWalletCommand command)
        {
            var customerId = await ResolveCustomerIdAsync();
            if (customerId is null) return BadRequest("Customer profile not found for this account");

            var response = await mediator.Send(command with { CustomerId = customerId.Value });
            return Ok(response);
        }

        [HttpPost("verify-wallet/{reference}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyWallet(string reference)
        {
            var response = await mediator.Send(new VerifyWalletFundingCommand(reference));
            return Ok(response);
        }

        [HttpGet("balance")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetBalance()
        {
            var customerId = await ResolveCustomerIdAsync();
            if (customerId is null) return BadRequest("Customer profile not found for this account");

            var response = await mediator.Send(new GetWalletBalanceQuery(customerId.Value));
            return Ok(response);
        }

        [HttpGet("history")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetHistory()
        {
            var customerId = await ResolveCustomerIdAsync();
            if (customerId is null) return BadRequest("Customer profile not found for this account");

            var response = await mediator.Send(new GetWalletTransactionHistoryQuery(customerId.Value));
            return Ok(response);
        }
    }
}