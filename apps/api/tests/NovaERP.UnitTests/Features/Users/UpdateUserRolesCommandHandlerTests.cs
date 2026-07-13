using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Authentication.Common;
using NovaERP.Application.Features.Users.UpdateUserRoles;
using NovaERP.Domain.Identity;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Users;

public sealed class UpdateUserRolesCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    /// <summary>
    /// Regresión del bug real de EF Core encontrado en la Fase 4: reasignar
    /// roles manteniendo uno, agregando otro y quitando un tercero en la misma
    /// operación generaba un DbUpdateConcurrencyException contra Postgres real
    /// (ver docs/PHASE-4-USERS-ROLES.md). Este test no reproduce el error de
    /// Npgsql en sí (el proveedor InMemory no lo dispara), pero sí protege que
    /// el resultado final de la reasignación sea el correcto.
    /// </summary>
    [Fact]
    public async Task Reassigning_roles_keeps_unchanged_adds_new_and_removes_dropped()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var kept = new Role(_tenantId, "Member");
        var added = new Role(_tenantId, "Admin");
        var dropped = new Role(_tenantId, "Vendedor");
        db.Roles.AddRange(kept, added, dropped);

        var user = new User(_tenantId, "user@acme.com", "hash", "Alguien");
        user.AssignRole(kept.Id);
        user.AssignRole(dropped.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new UpdateUserRolesCommandHandler(db);

        var result = await sut.Handle(
            new UpdateUserRolesCommand(user.Id, [kept.Id, added.Id]),
            CancellationToken.None);

        result.Roles.Select(r => r.Id).Should().BeEquivalentTo([kept.Id, added.Id]);
    }

    [Fact]
    public async Task Blocks_removing_owner_role_from_the_last_active_owner()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var owner = SystemRoles.CreateOwner(_tenantId);
        var member = new Role(_tenantId, "Member");
        db.Roles.AddRange(owner, member);

        var user = new User(_tenantId, "owner@acme.com", "hash", "Dueño");
        user.AssignRole(owner.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new UpdateUserRolesCommandHandler(db);

        var act = () => sut.Handle(new UpdateUserRolesCommand(user.Id, [member.Id]), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Allows_removing_owner_role_when_another_active_owner_remains()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var owner = SystemRoles.CreateOwner(_tenantId);
        var member = new Role(_tenantId, "Member");
        db.Roles.AddRange(owner, member);

        var user1 = new User(_tenantId, "owner1@acme.com", "hash", "Dueño Uno");
        user1.AssignRole(owner.Id);
        var user2 = new User(_tenantId, "owner2@acme.com", "hash", "Dueño Dos");
        user2.AssignRole(owner.Id);
        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new UpdateUserRolesCommandHandler(db);

        var result = await sut.Handle(new UpdateUserRolesCommand(user1.Id, [member.Id]), CancellationToken.None);

        result.Roles.Should().ContainSingle(r => r.Id == member.Id);
    }
}
