using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Authentication.Common;
using NovaERP.Application.Features.Users.Common;
using NovaERP.Domain.Identity;

namespace NovaERP.Application.Features.Users.UpdateUserRoles;

public sealed class UpdateUserRolesCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateUserRolesCommand, UserSummary>
{
    public async Task<UserSummary> Handle(UpdateUserRolesCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new ConflictException("El usuario no existe.");

        var currentAssignments = await db.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync(ct);

        var roles = await db.Roles.Where(r => request.RoleIds.Contains(r.Id)).ToListAsync(ct);
        if (roles.Count != request.RoleIds.Distinct().Count())
        {
            throw new ConflictException("Uno o más roles no existen en esta empresa.");
        }

        var ownerRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == SystemRoles.Owner, ct);
        var losingOwnerRole = ownerRole is not null
            && currentAssignments.Any(ur => ur.RoleId == ownerRole.Id)
            && !roles.Any(r => r.Id == ownerRole.Id);

        if (losingOwnerRole)
        {
            var remainingOwners = await db.Users
                .CountAsync(u => u.Id != user.Id && u.IsActive && u.Roles.Any(ur => ur.RoleId == ownerRole!.Id), ct);
            if (remainingOwners == 0)
            {
                throw new ConflictException("No puedes quitar el rol Owner al único dueño de la empresa.");
            }
        }

        // Se manipula la tabla de unión directamente vía su DbSet en vez de mutar la
        // colección de navegación de un agregado ya trackeado: mezclar en el mismo
        // contexto entidades cargadas por separado (usuario vs. roles) confunde el
        // fixup automático de EF y le hace generar un UPDATE en vez de un INSERT
        // para la fila nueva, disparando un falso conflicto de concurrencia.
        var targetRoleIds = roles.Select(r => r.Id).ToHashSet();
        var toRemove = currentAssignments.Where(ur => !targetRoleIds.Contains(ur.RoleId));
        db.UserRoles.RemoveRange(toRemove);

        var existingRoleIds = currentAssignments.Select(ur => ur.RoleId).ToHashSet();
        var toAdd = targetRoleIds.Where(roleId => !existingRoleIds.Contains(roleId));
        db.UserRoles.AddRange(toAdd.Select(roleId => new UserRole(user.Id, roleId)));

        await db.SaveChangesAsync(ct);

        return new UserSummary(
            user.Id, user.Email, user.FullName, user.IsActive,
            roles.Select(r => new RoleRef(r.Id, r.Name)).ToList());
    }
}
