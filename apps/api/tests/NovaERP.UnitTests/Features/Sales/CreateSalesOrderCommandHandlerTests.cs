using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Sales.CreateSalesOrder;
using NovaERP.Domain.Partners;
using NovaERP.Domain.Sales;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Sales;

public sealed class CreateSalesOrderCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Creates_a_draft_with_a_price_snapshot_and_does_not_touch_stock()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (customer, product) = await SalesTestData.SeedAsync(db, _tenantId, initialStock: 10, salePrice: 12.5m);

        var sut = new CreateSalesOrderCommandHandler(db, new FakeTenantProvider(_tenantId));
        var result = await sut.Handle(
            new CreateSalesOrderCommand(customer.Id, new DateOnly(2026, 7, 13), null,
                [new CreateSalesOrderLineInput(product.Id, 3)]),
            CancellationToken.None);

        result.OrderNumber.Should().Be("SO-00001");
        result.Status.Should().Be(nameof(SalesOrderStatus.Draft));
        result.TotalAmount.Should().Be(37.5m);
        result.Lines.Should().ContainSingle();
        result.Lines[0].ProductSku.Should().Be("SKU-1");
        result.Lines[0].UnitPrice.Should().Be(12.5m);

        // El borrador NO reserva stock.
        var stock = (await db.Products.FindAsync(product.Id))!.QuantityOnHand;
        stock.Should().Be(10);
    }

    [Fact]
    public async Task Rejects_selling_to_a_partner_that_is_not_a_customer()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (_, product) = await SalesTestData.SeedAsync(db, _tenantId);
        var supplier = new Partner(_tenantId, "Proveedor", PartnerType.Supplier);
        db.Partners.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new CreateSalesOrderCommandHandler(db, new FakeTenantProvider(_tenantId));
        var act = () => sut.Handle(
            new CreateSalesOrderCommand(supplier.Id, new DateOnly(2026, 7, 13), null,
                [new CreateSalesOrderLineInput(product.Id, 1)]),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
