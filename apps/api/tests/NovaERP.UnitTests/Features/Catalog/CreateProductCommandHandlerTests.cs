using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Catalog.CreateProduct;
using NovaERP.Domain.Catalog;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Catalog;

public sealed class CreateProductCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Creates_a_product_with_an_initial_quantity()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var sut = new CreateProductCommandHandler(db, new FakeTenantProvider(_tenantId));

        var result = await sut.Handle(
            new CreateProductCommand("SKU-1", "Agua", "unidad", 1.0m, 2.0m, null, null, null, InitialQuantity: 50),
            CancellationToken.None);

        result.QuantityOnHand.Should().Be(50);
        result.IsLowStock.Should().BeFalse();
    }

    [Fact]
    public async Task Flags_a_product_below_its_reorder_point_as_low_stock()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var sut = new CreateProductCommandHandler(db, new FakeTenantProvider(_tenantId));

        var result = await sut.Handle(
            new CreateProductCommand("SKU-1", "Agua", "unidad", 1.0m, 2.0m, null, null, ReorderPoint: 10, InitialQuantity: 5),
            CancellationToken.None);

        result.IsLowStock.Should().BeTrue();
    }

    [Fact]
    public async Task Rejects_duplicate_sku()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        db.Products.Add(new Product(_tenantId, "SKU-1", "Agua", "unidad", 1, 2));
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new CreateProductCommandHandler(db, new FakeTenantProvider(_tenantId));

        var act = () => sut.Handle(
            new CreateProductCommand("SKU-1", "Otra Agua", "unidad", 1, 2, null, null, null, 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Rejects_a_category_that_does_not_exist()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var sut = new CreateProductCommandHandler(db, new FakeTenantProvider(_tenantId));

        var act = () => sut.Handle(
            new CreateProductCommand("SKU-1", "Agua", "unidad", 1, 2, Guid.NewGuid(), null, null, 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
