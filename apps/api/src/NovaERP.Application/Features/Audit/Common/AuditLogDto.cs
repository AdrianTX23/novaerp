namespace NovaERP.Application.Features.Audit.Common;

public sealed record AuditLogDto(
    Guid Id,
    string EntityName,
    Guid EntityId,
    string Action,
    Guid? UserId,
    string? UserEmail,
    string? Changes,
    DateTimeOffset CreatedAt);

public sealed record AuditLogPageDto(IReadOnlyList<AuditLogDto> Items, int TotalCount);
