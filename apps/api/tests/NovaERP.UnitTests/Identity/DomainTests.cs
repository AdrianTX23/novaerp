using FluentAssertions;
using NovaERP.Domain.Identity;

namespace NovaERP.UnitTests.Identity;

public sealed class DomainTests
{
    [Fact]
    public void Permissions_catalog_has_unique_codes_and_ids()
    {
        Permissions.All.Select(p => p.Code).Should().OnlyHaveUniqueItems();
        Permissions.All.Select(p => p.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Owner_role_grants_every_permission()
    {
        var owner = SystemRolesOwnerFor(Guid.NewGuid());

        owner.Permissions.Should().HaveCount(Permissions.All.Count);
    }

    [Fact]
    public void User_AssignRole_is_idempotent()
    {
        var user = new User(Guid.NewGuid(), "a@b.com", "hash", "Test");
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId);
        user.AssignRole(roleId);

        user.Roles.Should().HaveCount(1);
    }

    [Fact]
    public void RefreshToken_is_inactive_after_revoke()
    {
        var token = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), "hash",
            DateTimeOffset.UtcNow.AddDays(7));

        token.IsActive.Should().BeTrue();
        token.Revoke();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_is_inactive_when_expired()
    {
        var token = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), "hash",
            DateTimeOffset.UtcNow.AddSeconds(-1));

        token.IsActive.Should().BeFalse();
    }

    private static Role SystemRolesOwnerFor(Guid tenantId) =>
        NovaERP.Application.Features.Authentication.Common.SystemRoles.CreateOwner(tenantId);
}
