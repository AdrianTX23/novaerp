using Microsoft.EntityFrameworkCore;
using NovaERP.Domain.Identity;

namespace NovaERP.Application.Common.Interfaces;

/// <summary>
/// Abstracción del DbContext expuesta a la capa Application. Mantiene a los
/// Handlers desacoplados de EF Core en Infrastructure sin recurrir a un
/// Repository genérico: los DbSet + SaveChangesAsync son suficientes.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
