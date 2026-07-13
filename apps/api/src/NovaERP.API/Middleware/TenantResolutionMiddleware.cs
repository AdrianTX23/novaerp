namespace NovaERP.API.Middleware;

/// <summary>
/// Corta el request con 401 si el usuario está autenticado pero su JWT no
/// trae el claim "tenant_id". Es la última barrera antes de que la request
/// llegue a un Handler: sin este middleware, ITenantProvider devolvería
/// Guid.Empty y el Global Query Filter simplemente no devolvería filas,
/// enmascarando un token mal emitido como "no hay datos".
/// </summary>
public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.User.FindFirst("tenant_id") is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token inválido: falta el claim 'tenant_id'.");
            return;
        }

        await next(context);
    }
}
