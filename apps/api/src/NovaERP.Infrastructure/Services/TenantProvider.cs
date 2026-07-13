using Microsoft.AspNetCore.Http;
using NovaERP.Application.Common.Interfaces;

namespace NovaERP.Infrastructure.Services;

/// <summary>
/// Lee el claim "tenant_id" del JWT del request actual. Registrado como
/// Scoped: se resuelve una vez por request y el DbContext lo captura al
/// construirse, garantizando que todo query de ese request quede filtrado.
/// </summary>
public sealed class TenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    public Guid TenantId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(claim, out var tenantId) ? tenantId : Guid.Empty;
        }
    }
}
