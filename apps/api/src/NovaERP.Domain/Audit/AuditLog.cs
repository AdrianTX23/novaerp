using NovaERP.Domain.Common;

namespace NovaERP.Domain.Audit;

public enum AuditAction
{
    Created,
    Updated,
    Deleted,
}

/// <summary>
/// Registro inmutable de un cambio de negocio, generado automáticamente por
/// AuditLogSaveChangesInterceptor en cada SaveChanges — nunca se crea a mano
/// desde un Handler. Solo de escritura: no se edita ni se borra.
/// </summary>
public sealed class AuditLog : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string EntityName { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public AuditAction Action { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }

    /// <summary>JSON de {Field, OldValue, NewValue}[]. Solo se llena para Updated.</summary>
    public string? Changes { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private AuditLog() { }

    public AuditLog(
        Guid tenantId,
        string entityName,
        Guid entityId,
        AuditAction action,
        Guid? userId,
        string? userEmail,
        string? changes,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        UserId = userId;
        UserEmail = userEmail;
        Changes = changes;
        CreatedAt = createdAt;
    }
}
