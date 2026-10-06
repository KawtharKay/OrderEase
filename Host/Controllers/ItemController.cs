using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.AddStock;
using static Application.Commands.CreateItem;
using static Application.Commands.DeleteItem;
using static Application.Commands.UpdateItem;
using static Application.Queries.GetAllItems;
using static Application.Queries.GetAllItemsForSupplier;
using static Application.Queries.GetItemsByCategory;
using static Application.Queries.GetStockMovementsByItem;

namespace Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController(IMediator mediator) : ControllerBase
    {
        [HttpPost("create-item")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> CreateItem([FromBody] CreateItemCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPatch("update-item/{id}")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> UpateItem(Guid id, [FromBody] UpdateItemCommand command)
        {
            var response = await mediator.Send(command with { Id = id });
            return Ok(response);
        }

        [HttpPost("{id}/add-stock")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> AddStock(Guid id, [FromBody] AddStockCommand command)
        {
            var response = await mediator.Send(command with { ItemId = id });
            return Ok(response);
        }

        [HttpGet("{id}/stock-movements")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> GetStockMovements(Guid id)
        {
            var response = await mediator.Send(new GetStockMovementsByItemQuery(id));
            return Ok(response);
        }

        [HttpDelete("delete-item/{id}")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var response = await mediator.Send(new DeleteItemCommand(id));
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItems([FromQuery] GetAllItemsQuery query)
        {
            var response = await mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("supplier")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> GetAllItemsForSupplier([FromQuery] GetAllItemsForSupplierQuery query)
        {
            var response = await mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetAllItemsByCategory(Guid categoryId)
        {
            var response = await mediator.Send(new GetItemsByCategoryQuery(categoryId));
            return Ok(response);
        }
    }
}
