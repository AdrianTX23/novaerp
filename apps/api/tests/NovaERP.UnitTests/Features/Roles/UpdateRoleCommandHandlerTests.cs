using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Authentication.Common;
using NovaERP.Application.Features.Roles.UpdateRole;
using NovaERP.Domain.Identity;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Roles;

public sealed class UpdateRoleCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    /// <summary>
    /// Mismo caso de regresión que UpdateUserRolesCommandHandlerTests, pero para
    /// permisos de rol: mantener uno, agregar otro y quitar un tercero.
    /// </summary>
    [Fact]
    public async Task Updating_permissions_keeps_unchanged_adds_new_and_revokes_dropped()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var role = new Role(_tenantId, "Vendedor");
        var salesRead = await db.Permissions.SingleAsync(p => p.Code == Permissions.SalesRead);
        var salesManage = await db.Permissions.SingleAsync(p => p.Code == Permissions.SalesManage);
        var reportsRead = await db.Permissions.SingleAsync(p => p.Code == Permissions.ReportsRead);
        role.GrantPermission(salesRead.Id);
        role.GrantPermission(salesManage.Id);
        db.Roles.Add(role);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new UpdateRoleCommandHandler(db);

        var result = await sut.Handle(
            new UpdateRoleCommand(role.Id, "Vendedor Senior", "Ventas + reportes",
                [Permissions.SalesRead, Permissions.ReportsRead]),
            CancellationToken.None);

        result.PermissionCodes.Should().BeEquivalentTo([Permissions.SalesRead, Permissions.ReportsRead]);
    }

    [Fact]
    public async Task Blocks_editing_a_system_role()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var owner = SystemRoles.CreateOwner(_tenantId);
        db.Roles.Add(owner);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new UpdateRoleCommandHandler(db);

        var act = () => sut.Handle(
            new UpdateRoleCommand(owner.Id, "Owner Hackeado", null, []), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
