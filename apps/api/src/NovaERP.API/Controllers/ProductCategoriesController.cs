using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Catalog.Common;
using NovaERP.Application.Features.Catalog.CreateCategory;
using NovaERP.Application.Features.Catalog.DeleteCategory;
using NovaERP.Application.Features.Catalog.ListCategories;
using NovaERP.Application.Features.Catalog.UpdateCategory;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/product-categories")]
[Authorize]
public sealed class ProductCategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.InventoryRead)]
    public async Task<ActionResult<List<CategoryDto>>> List(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListCategoriesQuery(), ct));
    }

    [HttpPost]
    [Authorize(Roles = Permissions.InventoryManage)]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        return Ok(await mediator.Send(new CreateCategoryCommand(request.Name, request.Description), ct));
    }

    [HttpPut("{categoryId:guid}")]
    [Authorize(Roles = Permissions.InventoryManage)]
    public async Task<ActionResult<CategoryDto>> Update(
        Guid categoryId, UpdateCategoryRequest request, CancellationToken ct)
    {
        return Ok(await mediator.Send(
            new UpdateCategoryCommand(categoryId, request.Name, request.Description), ct));
    }

    [HttpDelete("{categoryId:guid}")]
    [Authorize(Roles = Permissions.InventoryManage)]
    public async Task<IActionResult> Delete(Guid categoryId, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(categoryId), ct);
        return NoContent();
    }
}
