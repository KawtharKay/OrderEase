using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.InitiatePayment;
using static Application.Commands.VerifyPayment;
using static Application.Queries.GetAllCustomerBalances;
using static Application.Queries.GetCustomerOrdersWithPayments;
using static Application.Queries.GetPaymentByOrder;
using static Application.Queries.GetPaymentHistoryByCustomer;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController(IMediator mediator) : ControllerBase
    {
        [HttpPost("initiate")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> Initiate([FromBody] InitiatePaymentCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("verify/{reference}")]
        [AllowAnonymous] // Paystack calls this without your JWT token
        public async Task<IActionResult> Verify(string reference)
        {
            var response = await mediator.Send(new VerifyPaymentCommand(reference));
            return Ok(response);
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            using var json = System.Text.Json.JsonDocument.Parse(body);
            var eventType = json.RootElement.GetProperty("event").GetString();

            if (eventType == "charge.success")
            {
                var reference = json.RootElement
                    .GetProperty("data")
                    .GetProperty("reference")
                    .GetString();

                if (!string.IsNullOrEmpty(reference))
                {
                    await mediator.Send(new VerifyPaymentCommand(reference));
                }
            }

            return Ok();
        }

        [HttpGet("customer/{customerId}/history")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetHistory(Guid customerId)
        {
            var response = await mediator.Send(new GetPaymentHistoryByCustomerQuery(customerId));
            return Ok(response);
        }

        [HttpGet("balances")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> GetAllBalances()
        {
            var response = await mediator.Send(new GetAllCustomerBalancesQuery());
            return Ok(response);
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var response = await mediator.Send(new GetPaymentByOrderQuery(orderId));
            return Ok(response);
        }

        [HttpGet("customer/{customerId}/orders-summary")]
        public async Task<IActionResult> GetCustomerOrdersWithPayments(Guid customerId)
        {
            var response = await mediator.Send(new GetCustomerOrdersWithPaymentsQuery(customerId));
            return Ok(response);
        }
    }
}