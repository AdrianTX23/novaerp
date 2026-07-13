using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NovaERP.Application.Common.Interfaces;

namespace NovaERP.Infrastructure.Persistence;

/// <summary>
/// Factory usado solo por las herramientas de EF Core en tiempo de diseño
/// (dotnet ef migrations / database update). El DbContext en runtime recibe el
/// ITenantProvider real por DI; aquí basta un tenant vacío para inspeccionar el
/// modelo, ya que las migraciones no ejecutan queries filtradas.
/// </summary>
public sealed class NovaErpDbContextFactory : IDesignTimeDbContextFactory<NovaErpDbContext>
{
    public NovaErpDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("NOVAERP_MIGRATIONS_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=novaerp;Username=novaerp;Password=novaerp_dev";

        var options = new DbContextOptionsBuilder<NovaErpDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NovaErpDbContext(options, new DesignTimeTenantProvider());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid TenantId => Guid.Empty;
    }
}
