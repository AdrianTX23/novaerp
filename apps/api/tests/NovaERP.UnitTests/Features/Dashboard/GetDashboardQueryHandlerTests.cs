using FluentAssertions;
using NovaERP.Application.Features.Dashboard.GetDashboard;
using NovaERP.Application.Features.Sales.ConfirmSalesOrder;
using NovaERP.Application.Features.Sales.CreateSalesOrder;
using NovaERP.Domain.Catalog;
using NovaERP.Domain.Partners;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Dashboard;

public sealed class GetDashboardQueryHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    private async Task<Guid> ConfirmedSaleAsync(
        NovaERP.Infrastructure.Persistence.NovaErpDbContext db, Guid customerId, Guid productId, decimal qty)
    {
        var create = new CreateSalesOrderCommandHandler(db, new FakeTenantProvider(_tenantId));
        var order = await create.Handle(
            new CreateSalesOrderCommand(customerId, DateOnly.FromDateTime(DateTime.UtcNow), null,
                [new CreateSalesOrderLineInput(productId, qty)]),
            CancellationToken.None);
        await new ConfirmSalesOrderCommandHandler(db).Handle(new ConfirmSalesOrderCommand(order.Id), CancellationToken.None);
        return order.Id;
    }

    [Fact]
    public async Task Aggregates_only_confirmed_sales_and_computes_kpis()
    {
        using var db = TestDbContextFactory.Create(_tenantId);

        var acme = new Partner(_tenantId, "ACME", PartnerType.Customer);
        var globex = new Partner(_tenantId, "Globex", PartnerType.Customer);
        var laptop = new Product(_tenantId, "A", "Laptop", "u", costPrice: 500, salePrice: 900);
        var mouse = new Product(_tenantId, "B", "Mouse", "u", costPrice: 5, salePrice: 15);
        laptop.AdjustStock(50);
        mouse.AdjustStock(100);
        mouse.Update("Mouse", "u", 5, 15, null, null, reorderPoint: 200); // 100 <= 200 → bajo stock
        db.Partners.AddRange(acme, globex);
        db.Products.AddRange(laptop, mouse);
        await db.SaveChangesAsync(CancellationToken.None);

        await ConfirmedSaleAsync(db, acme.Id, laptop.Id, 3);   // 2700
        await ConfirmedSaleAsync(db, globex.Id, laptop.Id, 1); // 900

        // Un borrador (no confirmado) que NO debe contar.
        var create = new CreateSalesOrderCommandHandler(db, new FakeTenantProvider(_tenantId));
        await create.Handle(
            new CreateSalesOrderCommand(globex.Id, DateOnly.FromDateTime(DateTime.UtcNow), null,
                [new CreateSalesOrderLineInput(mouse.Id, 5)]),
            CancellationToken.None);

        var result = await new GetDashboardQueryHandler(db).Handle(new GetDashboardQuery(), CancellationToken.None);

        result.SalesThisMonth.Should().Be(3600);       // solo las 2 confirmadas
        result.SalesOrdersThisMonth.Should().Be(2);
        result.LowStockCount.Should().Be(1);            // Mouse
        result.InventoryValue.Should().Be(46 * 500 + 100 * 5); // Laptop 46u tras vender 4 + Mouse 100u
        result.TopCustomerName.Should().Be("ACME");
        result.TopCustomerRevenue.Should().Be(2700);
        result.TopProducts.Should().ContainSingle(p => p.ProductName == "Laptop" && p.QuantitySold == 4);
    }

    [Fact]
    public async Task Returns_zeros_for_a_tenant_without_activity()
    {
        using var db = TestDbContextFactory.Create(_tenantId);

        var result = await new GetDashboardQueryHandler(db).Handle(new GetDashboardQuery(), CancellationToken.None);

        result.SalesThisMonth.Should().Be(0);
        result.LowStockCount.Should().Be(0);
        result.InventoryValue.Should().Be(0);
        result.TopCustomerName.Should().BeNull();
        result.SalesTrend.Should().HaveCount(6);
        result.TopProducts.Should().BeEmpty();
    }
}
