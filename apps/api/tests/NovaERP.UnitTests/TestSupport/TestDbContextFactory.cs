using Microsoft.EntityFrameworkCore;
using NovaERP.Infrastructure.Persistence;
using NovaERP.Infrastructure.Persistence.Interceptors;

namespace NovaERP.UnitTests.TestSupport;

/// <summary>
/// Crea un NovaErpDbContext real (mismos Configuration/QueryFilters/Interceptor
/// que producción) sobre el proveedor InMemory de EF Core, para probar los
/// Handlers sin depender de una Postgres real. EnsureCreated() dispara el seed
/// de HasData() del catálogo de Permission, igual que la primera migración.
/// </summary>
public static class TestDbContextFactory
{
    public static NovaErpDbContext Create(
        Guid tenantId, FakeCurrentUserService? currentUser = null)
    {
        var tenantProvider = new FakeTenantProvider(tenantId);
        var interceptor = new AuditableEntitySaveChangesInterceptor(
            tenantProvider, currentUser ?? new FakeCurrentUserService());

        var options = new DbContextOptionsBuilder<NovaErpDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        var context = new NovaErpDbContext(options, tenantProvider);
        context.Database.EnsureCreated();
        return context;
    }
}
