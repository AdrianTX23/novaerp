using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Roles.Common;
using NovaERP.Application.Features.Roles.CreateRole;
using NovaERP.Application.Features.Roles.DeleteRole;
using NovaERP.Application.Features.Roles.ListPermissions;
using NovaERP.Application.Features.Roles.ListRoles;
using NovaERP.Application.Features.Roles.UpdateRole;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = Permissions.RolesManage)]
public sealed class RolesController(IMediator mediator) : ControllerBase
{
    [HttpGet("roles")]
    public async Task<ActionResult<List<RoleDetail>>> List(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListRolesQuery(), ct));
    }

    [HttpPost("roles")]
    public async Task<ActionResult<RoleDetail>> Create(CreateRoleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateRoleCommand(request.Name, request.Description, request.PermissionCodes), ct);
        return Ok(result);
    }

    [HttpPut("roles/{roleId:guid}")]
    public async Task<ActionResult<RoleDetail>> Update(Guid roleId, UpdateRoleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateRoleCommand(roleId, request.Name, request.Description, request.PermissionCodes), ct);
        return Ok(result);
    }

    [HttpDelete("roles/{roleId:guid}")]
    public async Task<IActionResult> Delete(Guid roleId, CancellationToken ct)
    {
        await mediator.Send(new DeleteRoleCommand(roleId), ct);
        return NoContent();
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<List<PermissionDto>>> ListPermissions(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListPermissionsQuery(), ct));
    }
}
