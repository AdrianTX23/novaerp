using FluentAssertions;
using NovaERP.Application.Features.Purchasing.CancelPurchaseOrder;
using NovaERP.Application.Features.Purchasing.ConfirmPurchaseOrder;
using NovaERP.Application.Features.Purchasing.CreatePurchaseOrder;
using NovaERP.Domain.Common.Exceptions;
using NovaERP.Domain.Purchasing;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Purchasing;

public sealed class ConfirmAndCancelPurchaseOrderTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    private async Task<Guid> CreateDraftAsync(NovaERP.Infrastructure.Persistence.NovaErpDbContext db,
        Guid supplierId, Guid productId, decimal quantity)
    {
        var create = new CreatePurchaseOrderCommandHandler(db, new FakeTenantProvider(_tenantId));
        var order = await create.Handle(
            new CreatePurchaseOrderCommand(supplierId, new DateOnly(2026, 7, 13), null,
                [new CreatePurchaseOrderLineInput(productId, quantity)]),
            CancellationToken.None);
        return order.Id;
    }

    [Fact]
    public async Task Confirm_increments_stock()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (supplier, product) = await PurchasingTestData.SeedAsync(db, _tenantId, initialStock: 0);
        var orderId = await CreateDraftAsync(db, supplier.Id, product.Id, 100);

        var sut = new ConfirmPurchaseOrderCommandHandler(db);
        var result = await sut.Handle(new ConfirmPurchaseOrderCommand(orderId), CancellationToken.None);

        result.Status.Should().Be(nameof(PurchaseOrderStatus.Confirmed));
        (await db.Products.FindAsync(product.Id))!.QuantityOnHand.Should().Be(100);
    }

    [Fact]
    public async Task Cancel_reverses_the_received_stock()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (supplier, product) = await PurchasingTestData.SeedAsync(db, _tenantId, initialStock: 0);
        var orderId = await CreateDraftAsync(db, supplier.Id, product.Id, 20);

        await new ConfirmPurchaseOrderCommandHandler(db).Handle(new ConfirmPurchaseOrderCommand(orderId), CancellationToken.None);
        (await db.Products.FindAsync(product.Id))!.QuantityOnHand.Should().Be(20);

        var sut = new CancelPurchaseOrderCommandHandler(db);
        var result = await sut.Handle(new CancelPurchaseOrderCommand(orderId), CancellationToken.None);

        result.Status.Should().Be(nameof(PurchaseOrderStatus.Cancelled));
        (await db.Products.FindAsync(product.Id))!.QuantityOnHand.Should().Be(0);
    }

    [Fact]
    public async Task Cancel_fails_when_received_stock_was_already_consumed()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (supplier, product) = await PurchasingTestData.SeedAsync(db, _tenantId, initialStock: 0);
        var orderId = await CreateDraftAsync(db, supplier.Id, product.Id, 100);
        await new ConfirmPurchaseOrderCommandHandler(db).Handle(new ConfirmPurchaseOrderCommand(orderId), CancellationToken.None);

        // Simula que ya se consumieron 60 de las 100 recibidas (p. ej. una venta).
        var product2 = (await db.Products.FindAsync(product.Id))!;
        product2.AdjustStock(-60);
        await db.SaveChangesAsync(CancellationToken.None);

        // Reversar 100 sobre un stock de 40 dejaría el saldo en negativo → rechazo.
        var sut = new CancelPurchaseOrderCommandHandler(db);
        var act = () => sut.Handle(new CancelPurchaseOrderCommand(orderId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
