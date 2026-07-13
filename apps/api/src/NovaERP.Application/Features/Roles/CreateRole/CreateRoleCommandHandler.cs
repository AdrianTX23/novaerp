using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Roles.Common;
using NovaERP.Domain.Identity;

namespace NovaERP.Application.Features.Roles.CreateRole;

public sealed class CreateRoleCommandHandler(
    IApplicationDbContext db,
    ITenantProvider tenantProvider) : IRequestHandler<CreateRoleCommand, RoleDetail>
{
    public async Task<RoleDetail> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var name = request.Name.Trim();

        var nameTaken = await db.Roles.AnyAsync(r => r.Name == name, ct);
        if (nameTaken)
        {
            throw new ConflictException("Ya existe un rol con ese nombre.");
        }

        var permissions = await db.Permissions
            .Where(p => request.PermissionCodes.Contains(p.Code))
            .ToListAsync(ct);
        if (permissions.Count != request.PermissionCodes.Distinct().Count())
        {
            throw new ConflictException("Uno o más permisos no existen.");
        }

        var role = new Role(tenantProvider.TenantId, name, request.Description?.Trim());
        foreach (var permission in permissions)
        {
            role.GrantPermission(permission.Id);
        }

        db.Roles.Add(role);
        await db.SaveChangesAsync(ct);

        return new RoleDetail(
            role.Id, role.Name, role.Description, role.IsSystem, 0,
            permissions.Select(p => p.Code).ToList());
    }
}
