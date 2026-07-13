using Microsoft.EntityFrameworkCore;
using NovaERP.Domain.Catalog;
using NovaERP.Domain.Identity;
using NovaERP.Domain.Partners;
using NovaERP.Domain.Purchasing;
using NovaERP.Domain.Sales;

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
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<Partner> Partners { get; }
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderLine> SalesOrderLines { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderLine> PurchaseOrderLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
