using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Users.SetUserActive;
using NovaERP.Domain.Identity;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Users;

public sealed class SetUserActiveCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Deactivates_another_user()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var target = new User(_tenantId, "target@acme.com", "hash", "Objetivo");
        db.Users.Add(target);
        await db.SaveChangesAsync(CancellationToken.None);

        var currentUser = new FakeCurrentUserService { UserId = Guid.NewGuid() };
        var sut = new SetUserActiveCommandHandler(db, currentUser);

        var result = await sut.Handle(new SetUserActiveCommand(target.Id, IsActive: false), CancellationToken.None);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Blocks_deactivating_your_own_account()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var self = new User(_tenantId, "self@acme.com", "hash", "Yo Mismo");
        db.Users.Add(self);
        await db.SaveChangesAsync(CancellationToken.None);

        var currentUser = new FakeCurrentUserService { UserId = self.Id };
        var sut = new SetUserActiveCommandHandler(db, currentUser);

        var act = () => sut.Handle(new SetUserActiveCommand(self.Id, IsActive: false), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Reactivates_a_deactivated_user()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var target = new User(_tenantId, "target@acme.com", "hash", "Objetivo");
        target.Deactivate();
        db.Users.Add(target);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new SetUserActiveCommandHandler(db, new FakeCurrentUserService { UserId = Guid.NewGuid() });

        var result = await sut.Handle(new SetUserActiveCommand(target.Id, IsActive: true), CancellationToken.None);

        result.IsActive.Should().BeTrue();
    }
}
