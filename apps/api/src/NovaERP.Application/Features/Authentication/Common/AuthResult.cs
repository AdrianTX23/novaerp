namespace NovaERP.Application.Features.Authentication.Common;

/// <summary>Resultado de un login/registro/refresh exitoso.</summary>
public sealed record AuthResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthUser User);

public sealed record AuthUser(
    Guid Id,
    Guid TenantId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
