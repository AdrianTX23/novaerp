using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Authentication.Common;
using NovaERP.Application.Features.Users.Common;
using NovaERP.Domain.Identity;

namespace NovaERP.Application.Features.Users.CreateUser;

public sealed class CreateUserCommandHandler(
    IApplicationDbContext db,
    ITenantProvider tenantProvider,
    IPasswordHasher passwordHasher) : IRequestHandler<CreateUserCommand, UserSummary>
{
    public async Task<UserSummary> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email, ct);
        if (emailTaken)
        {
            throw new ConflictException("Ya existe una cuenta con ese email.");
        }

        // El filtro global de tenant ya garantiza que solo se resuelvan roles de esta empresa.
        var roles = await db.Roles.Where(r => request.RoleIds.Contains(r.Id)).ToListAsync(ct);
        if (roles.Count != request.RoleIds.Distinct().Count())
        {
            throw new ConflictException("Uno o más roles no existen en esta empresa.");
        }

        var user = new User(
            tenantProvider.TenantId,
            email,
            passwordHasher.Hash(request.Password),
            request.FullName.Trim());

        foreach (var role in roles)
        {
            user.AssignRole(role.Id);
        }

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return new UserSummary(
            user.Id, user.Email, user.FullName, user.IsActive,
            roles.Select(r => new RoleRef(r.Id, r.Name)).ToList());
    }
}
