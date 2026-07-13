using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Users.Common;

namespace NovaERP.Application.Features.Users.ListUsers;

public sealed class ListUsersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListUsersQuery, List<UserSummary>>
{
    public async Task<List<UserSummary>> Handle(ListUsersQuery request, CancellationToken ct)
    {
        return await db.Users
            .OrderBy(u => u.FullName)
            .Select(u => new UserSummary(
                u.Id,
                u.Email,
                u.FullName,
                u.IsActive,
                u.Roles.Select(ur => new RoleRef(ur.Role.Id, ur.Role.Name)).ToList()))
            .ToListAsync(ct);
    }
}
