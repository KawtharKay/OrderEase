using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CreateCategory;
using static Application.Commands.DeleteCategory;
using static Application.Commands.UpdateCategory;
using static Application.Querries.GetAllCategories;

namespace Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(IMediator mediator) : ControllerBase
    {
        [HttpPost("create-category")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
        {
            var response = await mediator.Send(command);
            return Ok(response);
        }

        [HttpPatch("update-category/{id}")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> UpdateCategory(Guid id,[FromBody]string name)
        {
            var response = await mediator.Send(new UpdateCategoryCommand(id, name));
            return Ok(response);
        }

        [HttpDelete("delete-category/{id}")]
        [Authorize(Roles = AppRoles.Supplier)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var response = await mediator.Send(new DeleteCategoryCommand(id));
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] GetAllCategoriesQuery query)
        {
            var response = await mediator.Send(query);
            return Ok(response);
        }
    }

}
