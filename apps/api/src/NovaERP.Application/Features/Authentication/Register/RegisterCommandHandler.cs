using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Accounting.Common;
using NovaERP.Application.Features.Authentication.Common;
using NovaERP.Domain.Identity;

namespace NovaERP.Application.Features.Authentication.Register;

public sealed partial class RegisterCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, ct);
        if (emailTaken)
        {
            throw new ConflictException("Ya existe una cuenta con ese email.");
        }

        var slug = await GenerateUniqueSlugAsync(request.CompanyName, ct);

        var tenant = new Tenant(request.CompanyName.Trim(), slug);
        var ownerRole = SystemRoles.CreateOwner(tenant.Id);
        var adminRole = SystemRoles.CreateAdmin(tenant.Id);
        var memberRole = SystemRoles.CreateMember(tenant.Id);

        var user = new User(
            tenant.Id,
            email,
            passwordHasher.Hash(request.Password),
            request.FullName);
        user.AssignRole(ownerRole.Id);

        db.Tenants.Add(tenant);
        db.Roles.AddRange(ownerRole, adminRole, memberRole);
        db.Users.Add(user);
        db.Accounts.AddRange(DefaultChartOfAccounts.CreateFor(tenant.Id));
        await db.SaveChangesAsync(ct);

        var permissions = Permissions.All.Select(p => p.Code).ToList();
        var roles = new List<string> { SystemRoles.Owner };

        return await AuthResultFactory.CreateAsync(
            db, tokenService, user, roles, permissions, ct);
    }

    private async Task<string> GenerateUniqueSlugAsync(string companyName, CancellationToken ct)
    {
        var baseSlug = Slugify(companyName);
        var slug = baseSlug;
        var suffix = 1;

        while (await db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Slug == slug, ct))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static string Slugify(string value)
    {
        var lowered = value.Trim().ToLowerInvariant();
        var cleaned = NonSlugChars().Replace(lowered, "-").Trim('-');
        return string.IsNullOrEmpty(cleaned) ? "empresa" : cleaned;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugChars();
}
