using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Features.Reports.Common;
using NovaERP.Application.Features.Reports.GetInventoryReport;
using NovaERP.Application.Features.Reports.GetReceivablesReport;
using NovaERP.Application.Features.Reports.GetSalesReport;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = Permissions.ReportsRead)]
public sealed class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> Sales(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetSalesReportQuery(from, to), ct));
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<InventoryReportDto>> Inventory(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetInventoryReportQuery(), ct));
    }

    [HttpGet("receivables")]
    public async Task<ActionResult<ReceivablesReportDto>> Receivables(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetReceivablesReportQuery(), ct));
    }
}
