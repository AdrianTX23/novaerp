using MediatR;

namespace NovaERP.Application.Features.Roles.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId) : IRequest;
