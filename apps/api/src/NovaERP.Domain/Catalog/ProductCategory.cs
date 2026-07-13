using NovaERP.Domain.Common;

namespace NovaERP.Domain.Catalog;

public sealed class ProductCategory : TenantAuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private ProductCategory() { }

    public ProductCategory(Guid tenantId, string name, string? description = null)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name;
        Description = description;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
