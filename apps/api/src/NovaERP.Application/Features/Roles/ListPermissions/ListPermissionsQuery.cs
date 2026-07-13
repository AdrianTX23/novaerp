using MediatR;
using NovaERP.Application.Features.Roles.Common;

namespace NovaERP.Application.Features.Roles.ListPermissions;

public sealed record ListPermissionsQuery : IRequest<List<PermissionDto>>;
