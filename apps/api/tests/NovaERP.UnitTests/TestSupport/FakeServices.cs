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
