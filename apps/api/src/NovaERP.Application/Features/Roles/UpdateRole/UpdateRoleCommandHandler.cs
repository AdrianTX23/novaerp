using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Roles.Common;
using NovaERP.Domain.Identity;

namespace NovaERP.Application.Features.Roles.UpdateRole;

public sealed class UpdateRoleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateRoleCommand, RoleDetail>
{
    public async Task<RoleDetail> Handle(UpdateRoleCommand request, CancellationToken ct)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, ct)
            ?? throw new ConflictException("El rol no existe.");

        if (role.IsSystem)
        {
            throw new ConflictException("Los roles del sistema (Owner, Admin, Member) no se pueden editar.");
        }

        var name = request.Name.Trim();
        var nameTaken = await db.Roles.AnyAsync(r => r.Id != role.Id && r.Name == name, ct);
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

        role.Rename(name, request.Description?.Trim());

        // Ver UpdateUserRolesCommandHandler: se manipula la tabla de unión vía su
        // DbSet directamente, no mutando la colección de navegación del agregado.
        var currentGrants = await db.RolePermissions.Where(rp => rp.RoleId == role.Id).ToListAsync(ct);
        var targetPermissionIds = permissions.Select(p => p.Id).ToHashSet();

        var toRevoke = currentGrants.Where(rp => !targetPermissionIds.Contains(rp.PermissionId));
        db.RolePermissions.RemoveRange(toRevoke);

        var existingPermissionIds = currentGrants.Select(rp => rp.PermissionId).ToHashSet();
        var toGrant = targetPermissionIds.Where(id => !existingPermissionIds.Contains(id));
        db.RolePermissions.AddRange(toGrant.Select(id => new RolePermission(role.Id, id)));

        var userCount = await db.Users.CountAsync(u => u.Roles.Any(ur => ur.RoleId == role.Id), ct);

        await db.SaveChangesAsync(ct);

        return new RoleDetail(
            role.Id, role.Name, role.Description, role.IsSystem, userCount,
            permissions.Select(p => p.Code).ToList());
    }
}
