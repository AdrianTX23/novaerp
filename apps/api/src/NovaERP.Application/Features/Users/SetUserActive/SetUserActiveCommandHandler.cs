using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Users.Common;

namespace NovaERP.Application.Features.Users.SetUserActive;

public sealed class SetUserActiveCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<SetUserActiveCommand, UserSummary>
{
    public async Task<UserSummary> Handle(SetUserActiveCommand request, CancellationToken ct)
    {
        if (!request.IsActive && request.UserId == currentUser.UserId)
        {
            throw new ConflictException("No puedes desactivar tu propia cuenta.");
        }

        var user = await db.Users
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new ConflictException("El usuario no existe.");

        if (request.IsActive)
        {
            user.Reactivate();
        }
        else
        {
            user.Deactivate();
        }

        await db.SaveChangesAsync(ct);

        return new UserSummary(
            user.Id, user.Email, user.FullName, user.IsActive,
            user.Roles.Select(ur => new RoleRef(ur.Role.Id, ur.Role.Name)).ToList());
    }
}
