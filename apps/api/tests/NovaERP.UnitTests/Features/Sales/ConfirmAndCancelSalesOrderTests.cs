using FluentAssertions;
using NovaERP.Application.Features.Sales.CancelSalesOrder;
using NovaERP.Application.Features.Sales.ConfirmSalesOrder;
using NovaERP.Application.Features.Sales.CreateSalesOrder;
using NovaERP.Domain.Common.Exceptions;
using NovaERP.Domain.Sales;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Sales;

public sealed class ConfirmAndCancelSalesOrderTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    private async Task<Guid> CreateDraftAsync(NovaERP.Infrastructure.Persistence.NovaErpDbContext db,
        Guid customerId, Guid productId, decimal quantity)
    {
        var create = new CreateSalesOrderCommandHandler(db, new FakeTenantProvider(_tenantId), new FakeDocumentSequenceService());
        var order = await create.Handle(
            new CreateSalesOrderCommand(customerId, new DateOnly(2026, 7, 13), null,
                [new CreateSalesOrderLineInput(productId, quantity)]),
            CancellationToken.None);
        return order.Id;
    }

    [Fact]
    public async Task Confirm_decrements_stock()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (customer, product) = await SalesTestData.SeedAsync(db, _tenantId, initialStock: 10);
        var orderId = await CreateDraftAsync(db, customer.Id, product.Id, 3);

        var sut = new ConfirmSalesOrderCommandHandler(db);
        var result = await sut.Handle(new ConfirmSalesOrderCommand(orderId), CancellationToken.None);

        result.Status.Should().Be(nameof(SalesOrderStatus.Confirmed));
        (await db.Products.FindAsync(product.Id))!.QuantityOnHand.Should().Be(7);
    }

    [Fact]
    public async Task Confirm_fails_when_stock_is_insufficient()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (customer, product) = await SalesTestData.SeedAsync(db, _tenantId, initialStock: 2);
        var orderId = await CreateDraftAsync(db, customer.Id, product.Id, 5);

        var sut = new ConfirmSalesOrderCommandHandler(db);
        var act = () => sut.Handle(new ConfirmSalesOrderCommand(orderId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Cancel_restores_stock_of_a_confirmed_order()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (customer, product) = await SalesTestData.SeedAsync(db, _tenantId, initialStock: 10);
        var orderId = await CreateDraftAsync(db, customer.Id, product.Id, 4);

        await new ConfirmSalesOrderCommandHandler(db).Handle(new ConfirmSalesOrderCommand(orderId), CancellationToken.None);
        (await db.Products.FindAsync(product.Id))!.QuantityOnHand.Should().Be(6);

        var sut = new CancelSalesOrderCommandHandler(db);
        var result = await sut.Handle(new CancelSalesOrderCommand(orderId), CancellationToken.None);

        result.Status.Should().Be(nameof(SalesOrderStatus.Cancelled));
        (await db.Products.FindAsync(product.Id))!.QuantityOnHand.Should().Be(10);
    }

    [Fact]
    public async Task Cancel_of_a_draft_does_not_change_stock()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (customer, product) = await SalesTestData.SeedAsync(db, _tenantId, initialStock: 10);
        var orderId = await CreateDraftAsync(db, customer.Id, product.Id, 4);

        var sut = new CancelSalesOrderCommandHandler(db);
        var result = await sut.Handle(new CancelSalesOrderCommand(orderId), CancellationToken.None);

        result.Status.Should().Be(nameof(SalesOrderStatus.Cancelled));
        (await db.Products.FindAsync(product.Id))!.QuantityOnHand.Should().Be(10);
    }
}
