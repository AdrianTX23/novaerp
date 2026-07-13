using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Catalog.DeleteCategory;
using NovaERP.Domain.Catalog;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Catalog;

public sealed class DeleteCategoryCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Deletes_an_empty_category()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var category = new ProductCategory(_tenantId, "Bebidas");
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new DeleteCategoryCommandHandler(db);
        await sut.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        db.ProductCategories.Should().BeEmpty();
    }

    [Fact]
    public async Task Blocks_deleting_a_category_with_products()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var category = new ProductCategory(_tenantId, "Bebidas");
        db.ProductCategories.Add(category);
        db.Products.Add(new Product(_tenantId, "SKU-1", "Agua", "unidad", 1, 2, category.Id));
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new DeleteCategoryCommandHandler(db);
        var act = () => sut.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
