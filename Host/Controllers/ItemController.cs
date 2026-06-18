using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CreateItem;
using static Application.Commands.DeleteItem;
using static Application.Commands.UpdateItem;
using static Application.Queries.GetAllItems;
using static Application.Queries.GetItemsByCategory;

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
        public async Task<IActionResult> UpateItem([FromBody] Guid id, string title, string imageUrl, decimal price, int quantity)
        {
            var response = await mediator.Send(new UpdateItemCommand(id, title, imageUrl, price, quantity));
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

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetAllItemsByCategory(Guid categoryId)
        {
            var response = await mediator.Send(new GetItemsByCategoryQuery(categoryId));
            return Ok(response);
        }
    }
}
