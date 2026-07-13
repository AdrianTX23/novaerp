using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Purchasing.CancelPurchaseOrder;
using NovaERP.Application.Features.Purchasing.Common;
using NovaERP.Application.Features.Purchasing.ConfirmPurchaseOrder;
using NovaERP.Application.Features.Purchasing.CreatePurchaseOrder;
using NovaERP.Application.Features.Purchasing.GetPurchaseOrder;
using NovaERP.Application.Features.Purchasing.ListPurchaseOrders;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public sealed class PurchaseOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.PurchasesRead)]
    public async Task<ActionResult<List<PurchaseOrderSummary>>> List(
        [FromQuery] string? status, [FromQuery] Guid? supplierId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListPurchaseOrdersQuery(status, supplierId), ct));
    }

    [HttpGet("{orderId:guid}")]
    [Authorize(Roles = Permissions.PurchasesRead)]
    public async Task<ActionResult<PurchaseOrderDetail>> Get(Guid orderId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetPurchaseOrderQuery(orderId), ct));
    }

    [HttpPost]
    [Authorize(Roles = Permissions.PurchasesManage)]
    public async Task<ActionResult<PurchaseOrderDetail>> Create(CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        var lines = request.Lines
            .Select(l => new CreatePurchaseOrderLineInput(l.ProductId, l.Quantity))
            .ToList();

        var result = await mediator.Send(
            new CreatePurchaseOrderCommand(request.SupplierId, request.OrderDate, request.Notes, lines), ct);
        return Ok(result);
    }

    [HttpPost("{orderId:guid}/confirm")]
    [Authorize(Roles = Permissions.PurchasesManage)]
    public async Task<ActionResult<PurchaseOrderDetail>> Confirm(Guid orderId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new ConfirmPurchaseOrderCommand(orderId), ct));
    }

    [HttpPost("{orderId:guid}/cancel")]
    [Authorize(Roles = Permissions.PurchasesManage)]
    public async Task<ActionResult<PurchaseOrderDetail>> Cancel(Guid orderId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new CancelPurchaseOrderCommand(orderId), ct));
    }
}
