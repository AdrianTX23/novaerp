using MediatR;
using NovaERP.Application.Features.Roles.Common;

namespace NovaERP.Application.Features.Roles.ListRoles;

public sealed record ListRolesQuery : IRequest<List<RoleDetail>>;
