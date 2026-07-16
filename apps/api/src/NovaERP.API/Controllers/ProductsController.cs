using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Common;
using NovaERP.Application.Features.Catalog.AdjustStock;
using NovaERP.Application.Features.Catalog.Common;
using NovaERP.Application.Features.Catalog.CreateProduct;
using NovaERP.Application.Features.Catalog.ListProducts;
using NovaERP.Application.Features.Catalog.SetProductActive;
using NovaERP.Application.Features.Catalog.UpdateProduct;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.InventoryRead)]
    public async Task<ActionResult<PagedResult<ProductSummary>>> List(
        [FromQuery] string? search, [FromQuery] Guid? categoryId, [FromQuery] bool lowStockOnly,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var query = new ListProductsQuery(search, categoryId, lowStockOnly, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
        return Ok(await mediator.Send(query, ct));
    }

    [HttpPost]
    [Authorize(Roles = Permissions.InventoryManage)]
    public async Task<ActionResult<ProductSummary>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateProductCommand(
            request.Sku, request.Name, request.UnitOfMeasure, request.CostPrice, request.SalePrice,
            request.CategoryId, request.Description, request.ReorderPoint, request.InitialQuantity), ct);
        return Ok(result);
    }

    [HttpPut("{productId:guid}")]
    [Authorize(Roles = Permissions.InventoryManage)]
    public async Task<ActionResult<ProductSummary>> Update(
        Guid productId, UpdateProductRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateProductCommand(
            productId, request.Name, request.UnitOfMeasure, request.CostPrice, request.SalePrice,
            request.CategoryId, request.Description, request.ReorderPoint), ct);
        return Ok(result);
    }

    [HttpPost("{productId:guid}/adjust-stock")]
    [Authorize(Roles = Permissions.InventoryManage)]
    public async Task<ActionResult<ProductSummary>> AdjustStock(
        Guid productId, AdjustStockRequest request, CancellationToken ct)
    {
        return Ok(await mediator.Send(new AdjustStockCommand(productId, request.Delta), ct));
    }

    [HttpPost("{productId:guid}/deactivate")]
    [Authorize(Roles = Permissions.InventoryManage)]
    public async Task<ActionResult<ProductSummary>> Deactivate(Guid productId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new SetProductActiveCommand(productId, IsActive: false), ct));
    }

    [HttpPost("{productId:guid}/reactivate")]
    [Authorize(Roles = Permissions.InventoryManage)]
    public async Task<ActionResult<ProductSummary>> Reactivate(Guid productId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new SetProductActiveCommand(productId, IsActive: true), ct));
    }
}
