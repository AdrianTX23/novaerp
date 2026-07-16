using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Features.Settings.Common;
using NovaERP.Application.Features.Settings.GetCompany;
using NovaERP.Application.Features.Settings.RenameCompany;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public sealed class SettingsController(IMediator mediator) : ControllerBase
{
    /// <summary>Datos de la empresa del usuario autenticado. Cualquier miembro puede verlos.</summary>
    [HttpGet("company")]
    public async Task<ActionResult<CompanyDto>> GetCompany(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetCompanyQuery(), ct));
    }

    // Reutiliza users.manage en vez de un permiso nuevo: los permisos agregados
    // al catálogo después del registro no llegan retroactivamente al rol Owner
    // de tenants existentes, y administrar la empresa es coherente con
    // administrar sus usuarios.
    [HttpPut("company")]
    [Authorize(Roles = Permissions.UsersManage)]
    public async Task<ActionResult<CompanyDto>> RenameCompany(
        RenameCompanyCommand command,
        CancellationToken ct)
    {
        return Ok(await mediator.Send(command, ct));
    }
}
