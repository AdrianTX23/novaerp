using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Infrastructure.Persistence;

namespace NovaERP.IntegrationTests.TestSupport;

/// <summary>
/// Levanta la app completa (middleware, JWT, EF Core sobre Npgsql real) contra
/// el Postgres de desarrollo (docker-compose.yml / servicio de CI). Aplica las
/// migraciones una vez por ejecución de la suite, igual que haría un deploy.
/// </summary>
public sealed class NovaErpWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
        await db.Database.MigrateAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
