using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Authentication.Common;

namespace NovaERP.Application.Features.Authentication.Me;

public sealed class GetMeQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<GetMeQuery, AuthUser>
{
    public async Task<AuthUser> Handle(GetMeQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException("No autenticado.");

        // Request autenticado: el filtro global de tenant ya aísla al usuario.
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new UnauthorizedException("Usuario no encontrado.");

        var (roles, permissions) = await PermissionLoader.LoadAsync(db, user.Id, ct);

        return new AuthUser(
            user.Id, user.TenantId, user.Email, user.FullName, roles, permissions);
    }
}
