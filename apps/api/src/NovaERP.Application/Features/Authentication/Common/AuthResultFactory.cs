using NovaERP.Application.Common.Interfaces;
using NovaERP.Domain.Identity;

namespace NovaERP.Application.Features.Authentication.Common;

/// <summary>
/// Emite el par access+refresh token, persiste el refresh token (hasheado) y
/// arma el AuthResult. Centraliza el paso final común a Register, Login y Refresh.
/// </summary>
public static class AuthResultFactory
{
    public static async Task<AuthResult> CreateAsync(
        IApplicationDbContext db,
        ITokenService tokenService,
        User user,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions,
        CancellationToken ct)
    {
        var accessToken = tokenService.CreateAccessToken(user, permissions);
        var refresh = tokenService.CreateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken(
            user.TenantId, user.Id, refresh.Hash, refresh.ExpiresAt));
        await db.SaveChangesAsync(ct);

        return new AuthResult(
            accessToken.Value,
            accessToken.ExpiresAt,
            refresh.Value,
            refresh.ExpiresAt,
            new AuthUser(user.Id, user.TenantId, user.Email, user.FullName, roles, permissions));
    }
}
