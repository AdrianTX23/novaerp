using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Cash.Common;
using NovaERP.Application.Features.Cash.CreateCashMovement;
using NovaERP.Application.Features.Cash.DeleteCashMovement;
using NovaERP.Application.Features.Cash.GetCashSummary;
using NovaERP.Application.Features.Cash.ListCashMovements;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/cash")]
[Authorize]
public sealed class CashController(IMediator mediator) : ControllerBase
{
    [HttpGet("summary")]
    [Authorize(Roles = Permissions.CashRead)]
    public async Task<ActionResult<CashSummaryDto>> Summary(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetCashSummaryQuery(), ct));
    }

    [HttpGet("movements")]
    [Authorize(Roles = Permissions.CashRead)]
    public async Task<ActionResult<List<CashMovementDto>>> Movements(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListCashMovementsQuery(from, to), ct));
    }

    [HttpPost("movements")]
    [Authorize(Roles = Permissions.CashManage)]
    public async Task<ActionResult<CashMovementDto>> Create(CreateCashMovementRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCashMovementCommand(
            request.Type, request.Amount, request.Date, request.Concept, request.Description), ct);
        return Ok(result);
    }

    [HttpDelete("movements/{movementId:guid}")]
    [Authorize(Roles = Permissions.CashManage)]
    public async Task<IActionResult> Delete(Guid movementId, CancellationToken ct)
    {
        await mediator.Send(new DeleteCashMovementCommand(movementId), ct);
        return NoContent();
    }
}
