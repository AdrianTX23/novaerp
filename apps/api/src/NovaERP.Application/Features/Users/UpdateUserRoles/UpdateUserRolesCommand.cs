using MediatR;
using NovaERP.Application.Features.Users.Common;

namespace NovaERP.Application.Features.Users.UpdateUserRoles;

public sealed record UpdateUserRolesCommand(Guid UserId, List<Guid> RoleIds) : IRequest<UserSummary>;
