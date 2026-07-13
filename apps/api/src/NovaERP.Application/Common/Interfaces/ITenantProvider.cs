namespace NovaERP.Application.Common.Interfaces;

/// <summary>
/// Resuelve el tenant (empresa) del request actual a partir del claim
/// "tenant_id" del JWT. Implementado en Infrastructure vía IHttpContextAccessor
/// y consumido por los Global Query Filters del DbContext.
/// </summary>
public interface ITenantProvider
{
    Guid TenantId { get; }
}
