using NovaERP.Application.Common.Interfaces;

namespace NovaERP.UnitTests.TestSupport;

public sealed class FakeTenantProvider(Guid tenantId) : ITenantProvider
{
    public Guid TenantId { get; } = tenantId;
}

public sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; } = "test@novaerp.dev";
}

/// <summary>
/// La implementación real usa SQL crudo (INSERT ... ON CONFLICT) que el
/// proveedor InMemory de EF no soporta — este fake basta para los Handlers de
/// unit test, que corren en un solo hilo.
/// </summary>
public sealed class FakeDocumentSequenceService : IDocumentSequenceService
{
    private readonly Dictionary<(Guid, string), int> _counters = [];

    public Task<int> NextAsync(Guid tenantId, string documentType, CancellationToken ct)
    {
        var key = (tenantId, documentType);
        var next = _counters.GetValueOrDefault(key, 0) + 1;
        _counters[key] = next;
        return Task.FromResult(next);
    }
}
