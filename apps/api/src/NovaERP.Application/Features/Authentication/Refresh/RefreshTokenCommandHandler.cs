using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Authentication.Common;

namespace NovaERP.Application.Features.Authentication.Refresh;

public sealed class RefreshTokenCommandHandler(
    IApplicationDbContext db,
    ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);

        var stored = await db.RefreshTokens.IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

        if (stored is null)
        {
            throw new UnauthorizedException("Refresh token inválido.");
        }

        if (!stored.IsActive)
        {
            // Reutilización de un token ya revocado/expirado: posible robo.
            // Revocamos toda la cadena de sesiones activas del usuario por seguridad.
            await RevokeAllActiveTokensAsync(stored.UserId, ct);
            throw new UnauthorizedException("Refresh token expirado o revocado.");
        }

        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == stored.UserId, ct);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Usuario no disponible.");
        }

        // Rotación: el token usado se revoca y apunta al nuevo.
        var newRefresh = tokenService.CreateRefreshToken();
        stored.Revoke(newRefresh.Hash);

        var (roles, permissions) = await PermissionLoader.LoadAsync(db, user.Id, ct);

        var accessToken = tokenService.CreateAccessToken(user, permissions);
        db.RefreshTokens.Add(new Domain.Identity.RefreshToken(
            user.TenantId, user.Id, newRefresh.Hash, newRefresh.ExpiresAt));
        await db.SaveChangesAsync(ct);

        return new AuthResult(
            accessToken.Value,
            accessToken.ExpiresAt,
            newRefresh.Value,
            newRefresh.ExpiresAt,
            new AuthUser(user.Id, user.TenantId, user.Email, user.FullName, roles, permissions));
    }

    private async Task RevokeAllActiveTokensAsync(Guid userId, CancellationToken ct)
    {
        var active = await db.RefreshTokens.IgnoreQueryFilters()
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in active)
        {
            token.Revoke();
        }

        await db.SaveChangesAsync(ct);
    }
}
