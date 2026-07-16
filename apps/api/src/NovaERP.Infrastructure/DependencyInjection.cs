using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Authentication.Common;
using NovaERP.Infrastructure.Identity;
using NovaERP.Infrastructure.Persistence;
using NovaERP.Infrastructure.Persistence.Interceptors;
using NovaERP.Infrastructure.Services;
using StackExchange.Redis;

namespace NovaERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<AuditLogSaveChangesInterceptor>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey), "Jwt:SigningKey es obligatorio.")
            .ValidateOnStart();

        // Singleton (no Scoped): sin estado propio salvo el hash dummy cacheado
        // para el timing-guard de Login — si fuera Scoped, ese hash se
        // recalcularía en cada request sin usuario encontrado, reintroduciendo
        // el mismo tipo de diferencia de tiempo medible que el guard evita.
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Falta la connection string 'Postgres' en la configuración.");

        services.AddDbContext<NovaErpDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            // Orden importa: AuditLog debe ver el EntityState.Deleted real
            // antes de que el otro interceptor lo convierta en soft-delete.
            options.AddInterceptors(
                sp.GetRequiredService<AuditLogSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<NovaErpDbContext>());

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            // Conexión perezosa y tolerante a fallos: Redis es para caché/SignalR,
            // no ruta crítica. La factory (`sp => …`) evita conectar en el arranque
            // —solo al primer uso— y AbortOnConnectFail=false deja la app viva y
            // reintentando en segundo plano si Redis está caído.
            var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
            redisOptions.AbortOnConnectFail = false;
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
        }

        services.AddHangfire(config => config
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
        services.AddHangfireServer();

        return services;
    }
}
