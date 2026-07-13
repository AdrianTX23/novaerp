using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Users.CreateUser;
using NovaERP.Domain.Identity;
using NovaERP.Infrastructure.Identity;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Users;

public sealed class CreateUserCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Creates_user_with_the_requested_roles()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var role = new Role(_tenantId, "Member");
        db.Roles.Add(role);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new CreateUserCommandHandler(db, new FakeTenantProvider(_tenantId), new PasswordHasher());

        var result = await sut.Handle(
            new CreateUserCommand("new@acme.com", "Nueva Persona", "Test1234", [role.Id]),
            CancellationToken.None);

        result.Email.Should().Be("new@acme.com");
        result.IsActive.Should().BeTrue();
        result.Roles.Should().ContainSingle(r => r.Id == role.Id);
    }

    [Fact]
    public async Task Rejects_duplicate_email()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var role = new Role(_tenantId, "Member");
        db.Roles.Add(role);
        db.Users.Add(new User(_tenantId, "taken@acme.com", "hash", "Existente"));
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new CreateUserCommandHandler(db, new FakeTenantProvider(_tenantId), new PasswordHasher());

        var act = () => sut.Handle(
            new CreateUserCommand("taken@acme.com", "Otra Persona", "Test1234", [role.Id]),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Rejects_role_that_does_not_belong_to_the_tenant()
    {
        using var db = TestDbContextFactory.Create(_tenantId);

        var sut = new CreateUserCommandHandler(db, new FakeTenantProvider(_tenantId), new PasswordHasher());

        var act = () => sut.Handle(
            new CreateUserCommand("new@acme.com", "Nueva Persona", "Test1234", [Guid.NewGuid()]),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
