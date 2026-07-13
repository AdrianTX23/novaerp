using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Roles.Common;

namespace NovaERP.Application.Features.Roles.ListRoles;

public sealed class ListRolesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListRolesQuery, List<RoleDetail>>
{
    public async Task<List<RoleDetail>> Handle(ListRolesQuery request, CancellationToken ct)
    {
        return await db.Roles
            .OrderByDescending(r => r.IsSystem)
            .ThenBy(r => r.Name)
            .Select(r => new RoleDetail(
                r.Id,
                r.Name,
                r.Description,
                r.IsSystem,
                db.Users.Count(u => u.Roles.Any(ur => ur.RoleId == r.Id)),
                r.Permissions.Select(rp => rp.Permission.Code).ToList()))
            .ToListAsync(ct);
    }
}
