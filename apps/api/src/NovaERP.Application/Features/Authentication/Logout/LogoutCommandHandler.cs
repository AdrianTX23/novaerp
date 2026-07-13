using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Authentication.Common;

namespace NovaERP.Application.Features.Authentication.Logout;

public sealed class LogoutCommandHandler(
    IApplicationDbContext db,
    ITokenService tokenService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await db.RefreshTokens.IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash && rt.RevokedAt == null, ct);

        if (stored is not null)
        {
            stored.Revoke();
            await db.SaveChangesAsync(ct);
        }
    }
}
