using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;

namespace NovaERP.Application.Features.Roles.DeleteRole;

public sealed class DeleteRoleCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, ct)
            ?? throw new ConflictException("El rol no existe.");

        if (role.IsSystem)
        {
            throw new ConflictException("Los roles del sistema (Owner, Admin, Member) no se pueden eliminar.");
        }

        var hasUsers = await db.Users.AnyAsync(u => u.Roles.Any(ur => ur.RoleId == role.Id), ct);
        if (hasUsers)
        {
            throw new ConflictException("No puedes eliminar un rol que tiene usuarios asignados.");
        }

        db.Roles.Remove(role);
        await db.SaveChangesAsync(ct);
    }
}
