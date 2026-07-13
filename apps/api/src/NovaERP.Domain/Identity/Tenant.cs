using NovaERP.Domain.Common;

namespace NovaERP.Domain.Identity;

/// <summary>
/// La empresa cliente. Es la raíz del aislamiento multi-tenant: no implementa
/// ITenantEntity porque ES el tenant, no vive dentro de uno.
/// </summary>
public sealed class Tenant : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; } = null!;

    /// <summary>Identificador legible y único usado en URLs/subdominios (ej. "acme-inc").</summary>
    public string Slug { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    private Tenant() { }

    public Tenant(string name, string slug)
    {
        Id = Guid.NewGuid();
        Name = name;
        Slug = slug;
    }
}
