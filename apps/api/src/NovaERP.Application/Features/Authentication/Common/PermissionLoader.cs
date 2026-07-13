using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;

namespace NovaERP.Application.Features.Authentication.Common;

/// <summary>
/// Resuelve los códigos de permiso efectivos de un usuario (unión de todos sus
/// roles). Usa IgnoreQueryFilters porque el login/refresh corren sin tenant
/// autenticado todavía; el filtrado se hace explícito por UserId.
/// </summary>
public static class PermissionLoader
{
    public static async Task<(List<string> Roles, List<string> Permissions)> LoadAsync(
        IApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var roles = await db.Users.IgnoreQueryFilters()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .Select(ur => ur.Role.Name)
            .Distinct()
            .ToListAsync(ct);

        var permissions = await db.Users.IgnoreQueryFilters()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(ur => ur.Role.Permissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(ct);

        return (roles, permissions);
    }
}
