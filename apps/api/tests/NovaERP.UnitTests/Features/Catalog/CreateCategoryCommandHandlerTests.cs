using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Catalog.CreateCategory;
using NovaERP.Domain.Catalog;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Catalog;

public sealed class CreateCategoryCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Creates_a_category()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var sut = new CreateCategoryCommandHandler(db, new FakeTenantProvider(_tenantId));

        var result = await sut.Handle(new CreateCategoryCommand("Bebidas", "Refrescos y jugos"), CancellationToken.None);

        result.Name.Should().Be("Bebidas");
        result.ProductCount.Should().Be(0);
    }

    [Fact]
    public async Task Rejects_duplicate_category_name()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        db.ProductCategories.Add(new ProductCategory(_tenantId, "Bebidas"));
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new CreateCategoryCommandHandler(db, new FakeTenantProvider(_tenantId));

        var act = () => sut.Handle(new CreateCategoryCommand("Bebidas", null), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
