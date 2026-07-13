using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Roles.CreateRole;
using NovaERP.Domain.Identity;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Roles;

public sealed class CreateRoleCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Creates_role_with_the_requested_permissions()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var sut = new CreateRoleCommandHandler(db, new FakeTenantProvider(_tenantId));

        var result = await sut.Handle(
            new CreateRoleCommand("Vendedor", "Solo ventas", [Permissions.SalesRead, Permissions.SalesManage]),
            CancellationToken.None);

        result.IsSystem.Should().BeFalse();
        result.PermissionCodes.Should().BeEquivalentTo([Permissions.SalesRead, Permissions.SalesManage]);
    }

    [Fact]
    public async Task Rejects_duplicate_role_name_within_the_tenant()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        db.Roles.Add(new Role(_tenantId, "Vendedor"));
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new CreateRoleCommandHandler(db, new FakeTenantProvider(_tenantId));

        var act = () => sut.Handle(
            new CreateRoleCommand("Vendedor", null, [Permissions.SalesRead]), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Rejects_unknown_permission_code()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var sut = new CreateRoleCommandHandler(db, new FakeTenantProvider(_tenantId));

        var act = () => sut.Handle(
            new CreateRoleCommand("Vendedor", null, ["no.existe"]), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
