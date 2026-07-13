using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Roles.Common;

namespace NovaERP.Application.Features.Roles.ListPermissions;

public sealed class ListPermissionsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListPermissionsQuery, List<PermissionDto>>
{
    public async Task<List<PermissionDto>> Handle(ListPermissionsQuery request, CancellationToken ct)
    {
        return await db.Permissions
            .OrderBy(p => p.Group).ThenBy(p => p.Code)
            .Select(p => new PermissionDto(p.Id, p.Code, p.Description, p.Group))
            .ToListAsync(ct);
    }
}
