using MediatR;
using NovaERP.Application.Features.Roles.Common;

namespace NovaERP.Application.Features.Roles.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    List<string> PermissionCodes) : IRequest<RoleDetail>;
