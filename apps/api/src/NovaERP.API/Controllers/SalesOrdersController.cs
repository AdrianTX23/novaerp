using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Sales.CancelSalesOrder;
using NovaERP.Application.Features.Sales.Common;
using NovaERP.Application.Features.Sales.ConfirmSalesOrder;
using NovaERP.Application.Features.Sales.CreateSalesOrder;
using NovaERP.Application.Features.Sales.GetSalesOrder;
using NovaERP.Application.Features.Sales.ListSalesOrders;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/sales-orders")]
[Authorize]
public sealed class SalesOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.SalesRead)]
    public async Task<ActionResult<List<SalesOrderSummary>>> List(
        [FromQuery] string? status, [FromQuery] Guid? customerId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListSalesOrdersQuery(status, customerId), ct));
    }

    [HttpGet("{orderId:guid}")]
    [Authorize(Roles = Permissions.SalesRead)]
    public async Task<ActionResult<SalesOrderDetail>> Get(Guid orderId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetSalesOrderQuery(orderId), ct));
    }

    [HttpPost]
    [Authorize(Roles = Permissions.SalesManage)]
    public async Task<ActionResult<SalesOrderDetail>> Create(CreateSalesOrderRequest request, CancellationToken ct)
    {
        var lines = request.Lines
            .Select(l => new CreateSalesOrderLineInput(l.ProductId, l.Quantity))
            .ToList();

        var result = await mediator.Send(
            new CreateSalesOrderCommand(request.CustomerId, request.OrderDate, request.Notes, lines), ct);
        return Ok(result);
    }

    [HttpPost("{orderId:guid}/confirm")]
    [Authorize(Roles = Permissions.SalesManage)]
    public async Task<ActionResult<SalesOrderDetail>> Confirm(Guid orderId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new ConfirmSalesOrderCommand(orderId), ct));
    }

    [HttpPost("{orderId:guid}/cancel")]
    [Authorize(Roles = Permissions.SalesManage)]
    public async Task<ActionResult<SalesOrderDetail>> Cancel(Guid orderId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new CancelSalesOrderCommand(orderId), ct));
    }
}
