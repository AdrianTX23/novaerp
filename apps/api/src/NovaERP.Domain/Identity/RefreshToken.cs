using NovaERP.Domain.Common;

namespace NovaERP.Domain.Identity;

/// <summary>
/// Refresh token persistido para poder revocar sesiones. Se rota en cada uso:
/// al refrescar, el token viejo se marca revocado y apunta al que lo reemplaza,
/// dejando una cadena auditable y detectando reutilización de tokens robados.
/// </summary>
public sealed class RefreshToken : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; private set; }

    /// <summary>Hash SHA-256 del token. Nunca se guarda el valor en claro.</summary>
    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;

    private RefreshToken() { }

    public RefreshToken(Guid tenantId, Guid userId, string tokenHash, DateTimeOffset expiresAt)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
