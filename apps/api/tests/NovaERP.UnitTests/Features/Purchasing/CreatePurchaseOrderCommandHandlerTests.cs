using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Purchasing.CreatePurchaseOrder;
using NovaERP.Domain.Partners;
using NovaERP.Domain.Purchasing;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Purchasing;

public sealed class CreatePurchaseOrderCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Creates_a_draft_with_a_cost_snapshot_and_does_not_touch_stock()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (supplier, product) = await PurchasingTestData.SeedAsync(db, _tenantId, initialStock: 0, costPrice: 5m);

        var sut = new CreatePurchaseOrderCommandHandler(db, new FakeTenantProvider(_tenantId), new FakeDocumentSequenceService());
        var result = await sut.Handle(
            new CreatePurchaseOrderCommand(supplier.Id, new DateOnly(2026, 7, 13), null,
                [new CreatePurchaseOrderLineInput(product.Id, 100)]),
            CancellationToken.None);

        result.OrderNumber.Should().Be("PO-00001");
        result.Status.Should().Be(nameof(PurchaseOrderStatus.Draft));
        result.TotalAmount.Should().Be(500m);
        result.Lines.Should().ContainSingle();
        result.Lines[0].UnitCost.Should().Be(5m);

        // El borrador NO ingresa stock.
        (await db.Products.FindAsync(product.Id))!.QuantityOnHand.Should().Be(0);
    }

    [Fact]
    public async Task Rejects_buying_from_a_partner_that_is_not_a_supplier()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (_, product) = await PurchasingTestData.SeedAsync(db, _tenantId);
        var customer = new Partner(_tenantId, "Cliente", PartnerType.Customer);
        db.Partners.Add(customer);
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new CreatePurchaseOrderCommandHandler(db, new FakeTenantProvider(_tenantId), new FakeDocumentSequenceService());
        var act = () => sut.Handle(
            new CreatePurchaseOrderCommand(customer.Id, new DateOnly(2026, 7, 13), null,
                [new CreatePurchaseOrderLineInput(product.Id, 1)]),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
