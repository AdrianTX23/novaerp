using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Domain.Catalog;
using NovaERP.Domain.Common;
using NovaERP.Domain.Identity;
using NovaERP.Domain.Partners;

namespace NovaERP.Infrastructure.Persistence;

/// <summary>
/// DbContext único de NovaERP. Aplica automáticamente el filtro de tenant y
/// el filtro de soft-delete a toda entidad que implemente ITenantEntity /
/// ISoftDeletable — ningún módulo de negocio necesita repetir ese filtro.
/// </summary>
public sealed class NovaErpDbContext(
    DbContextOptions<NovaErpDbContext> options,
    ITenantProvider tenantProvider) : DbContext(options), IApplicationDbContext
{
    // IMPORTANTE: los filtros globales referencian este campo de INSTANCIA dentro
    // de un lambda de C# (no via Expression.Constant). EF cachea el modelo una
    // sola vez, pero al ver el acceso al campo lo trata como parámetro re-evaluado
    // en cada query. Hornear el valor como constante congelaría el tenant de la
    // primera petición (típicamente Guid.Empty en el arranque) para siempre.
    private readonly Guid _tenantId = tenantProvider.TenantId;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Partner> Partners => Set<Partner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NovaErpDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var isTenant = typeof(ITenantEntity).IsAssignableFrom(clrType);
            var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(clrType);

            if (isTenant)
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(ITenantEntity.TenantId));
            }

            var method = (isTenant, isSoftDeletable) switch
            {
                (true, true) => TenantAndSoftDeleteMethod.MakeGenericMethod(clrType),
                (true, false) => TenantOnlyMethod.MakeGenericMethod(clrType),
                (false, true) => SoftDeleteOnlyMethod.MakeGenericMethod(clrType),
                _ => null,
            };

            method?.Invoke(this, [modelBuilder]);
        }

        base.OnModelCreating(modelBuilder);
    }

    private static readonly MethodInfo TenantAndSoftDeleteMethod =
        typeof(NovaErpDbContext).GetMethod(nameof(ApplyTenantAndSoftDeleteFilter), BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo TenantOnlyMethod =
        typeof(NovaErpDbContext).GetMethod(nameof(ApplyTenantFilter), BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo SoftDeleteOnlyMethod =
        typeof(NovaErpDbContext).GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private void ApplyTenantAndSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity, ISoftDeletable
        => modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
        => modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == _tenantId);

    private void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletable
        => modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
}
