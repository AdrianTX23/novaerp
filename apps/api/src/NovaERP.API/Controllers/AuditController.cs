using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Features.Audit.Common;
using NovaERP.Application.Features.Audit.ListAuditLogs;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

// Reutiliza users.manage (igual que SettingsController): un permiso nuevo
// "audit.read" no llegaría retroactivamente al rol Owner de tenants ya
// registrados, y ver la auditoría es coherente con administrar la empresa.
[ApiController]
[Route("api/audit")]
[Authorize(Roles = Permissions.UsersManage)]
public sealed class AuditController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AuditLogPageDto>> List(
        [FromQuery] string? entityName,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var query = new ListAuditLogsQuery(entityName, from, to, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
        return Ok(await mediator.Send(query, ct));
    }
}
