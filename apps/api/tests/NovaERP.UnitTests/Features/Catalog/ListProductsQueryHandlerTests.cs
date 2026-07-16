using FluentAssertions;
using NovaERP.Application.Features.Catalog.ListProducts;
using NovaERP.Domain.Catalog;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Catalog;

public sealed class ListProductsQueryHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task LowStockOnly_returns_only_products_at_or_below_their_reorder_point()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var low = new Product(_tenantId, "SKU-LOW", "Bajo Stock", "unidad", 1, 2, reorderPoint: 10);
        low.AdjustStock(5);
        var ok = new Product(_tenantId, "SKU-OK", "Stock Normal", "unidad", 1, 2, reorderPoint: 10);
        ok.AdjustStock(50);
        db.Products.AddRange(low, ok);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new ListProductsQueryHandler(db);
        var result = await sut.Handle(new ListProductsQuery(LowStockOnly: true), CancellationToken.None);

        result.Items.Should().ContainSingle(p => p.Sku == "SKU-LOW");
    }

    [Fact]
    public async Task Search_matches_by_name_or_sku_case_insensitively()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        db.Products.Add(new Product(_tenantId, "AGUA-500", "Agua Mineral", "unidad", 1, 2));
        db.Products.Add(new Product(_tenantId, "GASEOSA-1", "Gaseosa Cola", "unidad", 1, 2));
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new ListProductsQueryHandler(db);
        var result = await sut.Handle(new ListProductsQuery(Search: "agua"), CancellationToken.None);

        result.Items.Should().ContainSingle(p => p.Sku == "AGUA-500");
    }
}
