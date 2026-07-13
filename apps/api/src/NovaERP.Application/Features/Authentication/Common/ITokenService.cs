using NovaERP.Domain.Identity;

namespace NovaERP.Application.Features.Authentication.Common;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
public sealed record RefreshTokenValue(string Value, string Hash, DateTimeOffset ExpiresAt);

/// <summary>
/// Emite el access token JWT (con claims sub/email/tenant_id/permission) y
/// genera refresh tokens criptográficamente seguros junto con su hash.
/// </summary>
public interface ITokenService
{
    AccessToken CreateAccessToken(User user, IReadOnlyList<string> permissions);
    RefreshTokenValue CreateRefreshToken();
    string HashRefreshToken(string rawToken);
}
