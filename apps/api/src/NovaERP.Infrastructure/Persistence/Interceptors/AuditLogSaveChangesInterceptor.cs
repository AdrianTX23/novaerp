using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Domain.Audit;
using NovaERP.Domain.Common;
using NovaERP.Domain.Identity;

namespace NovaERP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Antes de cada SaveChanges: por cada entidad de negocio creada, modificada o
/// eliminada, agrega una fila de AuditLog a la misma transacción. Se registra
/// ANTES que AuditableEntitySaveChangesInterceptor (ver DependencyInjection)
/// para observar el EntityState real (Deleted) antes de que ese interceptor lo
/// convierta en soft-delete (Modified + IsDeleted=true).
///
/// Auditar es automático por reflexión, igual que el filtro de tenant: no hace
/// falta tocar este archivo cuando se agrega una entidad de negocio nueva.
/// Solo las entidades de línea (SalesOrderLine, InvoiceLine, etc.) quedan
/// fuera porque no implementan ITenantEntity — evita ruido a nivel de detalle.
/// </summary>
public sealed class AuditLogSaveChangesInterceptor(
    ITenantProvider tenantProvider,
    ICurrentUserService currentUser) : SaveChangesInterceptor
{
    // Entidades técnicas: no son datos de negocio (RefreshToken) o auditarse a
    // sí misma causaría recursión (AuditLog).
    private static readonly HashSet<Type> Excluded = [typeof(RefreshToken), typeof(AuditLog)];

    // Campos que nunca se muestran en el diff: los que ya administra el otro
    // interceptor (redundantes) y PasswordHash (nunca se loguea un hash, ni
    // interno — no hay ninguna razón de negocio para verlo y es un riesgo).
    private static readonly HashSet<string> IgnoredProperties =
    [
        nameof(BaseEntity.Id),
        nameof(ITenantEntity.TenantId),
        nameof(IAuditableEntity.CreatedAt),
        nameof(IAuditableEntity.CreatedBy),
        nameof(IAuditableEntity.LastModifiedAt),
        nameof(IAuditableEntity.LastModifiedBy),
        nameof(ISoftDeletable.IsDeleted),
        nameof(ISoftDeletable.DeletedAt),
        nameof(User.PasswordHash),
    ];

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AppendAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AppendAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // El resto del API serializa en camelCase (convención global de ASP.NET);
    // este JSON se serializa a mano y viaja tal cual al frontend, así que debe
    // seguir la misma convención o el cliente no reconoce los campos.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private void AppendAuditLogs(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var tenantId = tenantProvider.TenantId;
        var now = DateTimeOffset.UtcNow;

        // Snapshot: agregar filas de AuditLog mientras se recorre el
        // ChangeTracker invalidaría la enumeración en curso.
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity
                && (e.Entity is ITenantEntity || e.Entity is Tenant)
                && !Excluded.Contains(e.Entity.GetType())
                && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            string? changes = null;
            AuditAction action;

            switch (entry.State)
            {
                case EntityState.Added:
                    action = AuditAction.Created;
                    break;
                case EntityState.Deleted:
                    action = AuditAction.Deleted;
                    break;
                default:
                    var diffs = entry.Properties
                        .Where(p => p.IsModified && !IgnoredProperties.Contains(p.Metadata.Name))
                        .Select(p => new FieldChange(p.Metadata.Name, p.OriginalValue?.ToString(), p.CurrentValue?.ToString()))
                        .ToList();
                    if (diffs.Count == 0)
                    {
                        continue; // nada relevante cambió (ej. solo timestamps)
                    }
                    action = AuditAction.Updated;
                    changes = JsonSerializer.Serialize(diffs, JsonOptions);
                    break;
            }

            var entityId = ((BaseEntity)entry.Entity).Id;
            var entityName = entry.Entity.GetType().Name;

            context.Add(new AuditLog(tenantId, entityName, entityId, action, currentUser.UserId, currentUser.Email, changes, now));
        }
    }

    private sealed record FieldChange(string Field, string? OldValue, string? NewValue);
}
