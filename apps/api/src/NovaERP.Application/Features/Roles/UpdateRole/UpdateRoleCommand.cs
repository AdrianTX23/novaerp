using MediatR;
using NovaERP.Application.Features.Roles.Common;

namespace NovaERP.Application.Features.Roles.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string? Description,
    List<string> PermissionCodes) : IRequest<RoleDetail>;
