using FluentAssertions;
using NovaERP.Application.Features.Catalog.AdjustStock;
using NovaERP.Domain.Catalog;
using NovaERP.Domain.Common.Exceptions;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Catalog;

public sealed class AdjustStockCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Increases_stock_with_a_positive_delta()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var product = new Product(_tenantId, "SKU-1", "Agua", "unidad", 1, 2);
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new AdjustStockCommandHandler(db);
        var result = await sut.Handle(new AdjustStockCommand(product.Id, 20), CancellationToken.None);

        result.QuantityOnHand.Should().Be(20);
    }

    [Fact]
    public async Task Rejects_an_adjustment_that_would_leave_stock_negative()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var product = new Product(_tenantId, "SKU-1", "Agua", "unidad", 1, 2);
        product.AdjustStock(5);
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new AdjustStockCommandHandler(db);
        var act = () => sut.Handle(new AdjustStockCommand(product.Id, -10), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
