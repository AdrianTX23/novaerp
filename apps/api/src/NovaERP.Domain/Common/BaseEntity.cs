namespace NovaERP.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}

/// <summary>
/// Entidad base para todo agregado de negocio de NovaERP: aislada por tenant,
/// auditada automáticamente y con soft-delete. El SaveChangesInterceptor de
/// Infrastructure rellena estos campos — nunca se asignan a mano en un Handler.
/// </summary>
public abstract class TenantAuditableEntity : BaseEntity, ITenantEntity, IAuditableEntity, ISoftDeletable
{
    public Guid TenantId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
