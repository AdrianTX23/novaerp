using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Authentication.Common;
using NovaERP.Application.Features.Roles.DeleteRole;
using NovaERP.Domain.Identity;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Roles;

public sealed class DeleteRoleCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Soft_deletes_a_role_with_no_users_assigned()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var role = new Role(_tenantId, "Vendedor");
        db.Roles.Add(role);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new DeleteRoleCommandHandler(db);
        await sut.Handle(new DeleteRoleCommand(role.Id), CancellationToken.None);

        (await db.Roles.AnyAsync(r => r.Id == role.Id)).Should().BeFalse();
        (await db.Roles.IgnoreQueryFilters().SingleAsync(r => r.Id == role.Id)).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Blocks_deleting_a_system_role()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var owner = SystemRoles.CreateOwner(_tenantId);
        db.Roles.Add(owner);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new DeleteRoleCommandHandler(db);
        var act = () => sut.Handle(new DeleteRoleCommand(owner.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Blocks_deleting_a_role_with_users_assigned()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var role = new Role(_tenantId, "Vendedor");
        db.Roles.Add(role);
        var user = new User(_tenantId, "user@acme.com", "hash", "Alguien");
        user.AssignRole(role.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new DeleteRoleCommandHandler(db);
        var act = () => sut.Handle(new DeleteRoleCommand(role.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
