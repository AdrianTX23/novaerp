using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Invoicing.CreateInvoice;
using NovaERP.Application.Features.Invoicing.RegisterPayment;
using NovaERP.Application.Features.Invoicing.VoidInvoice;
using NovaERP.Application.Features.Sales.ConfirmSalesOrder;
using NovaERP.Application.Features.Sales.CreateSalesOrder;
using NovaERP.Domain.Catalog;
using NovaERP.Domain.Invoicing;
using NovaERP.Domain.Partners;
using NovaERP.Infrastructure.Persistence;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Invoicing;

public sealed class InvoicingHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Crea un cliente, un producto con stock y un pedido de venta confirmado; devuelve el orderId.</summary>
    private async Task<Guid> ConfirmedOrderAsync(NovaErpDbContext db, decimal qty, decimal salePrice)
    {
        var customer = new Partner(_tenantId, "ACME", PartnerType.Customer);
        var product = new Product(_tenantId, "SKU-1", "Laptop", "u", costPrice: 500, salePrice: salePrice);
        product.AdjustStock(100);
        db.Partners.Add(customer);
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None);

        var order = await new CreateSalesOrderCommandHandler(db, new FakeTenantProvider(_tenantId)).Handle(
            new CreateSalesOrderCommand(customer.Id, Today, null, [new CreateSalesOrderLineInput(product.Id, qty)]),
            CancellationToken.None);
        await new ConfirmSalesOrderCommandHandler(db).Handle(new ConfirmSalesOrderCommand(order.Id), CancellationToken.None);
        return order.Id;
    }

    [Fact]
    public async Task Creates_an_invoice_from_a_confirmed_order_with_a_line_snapshot()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var orderId = await ConfirmedOrderAsync(db, qty: 3, salePrice: 900);

        var result = await new CreateInvoiceCommandHandler(db, new FakeTenantProvider(_tenantId))
            .Handle(new CreateInvoiceCommand(orderId, null, null), CancellationToken.None);

        result.InvoiceNumber.Should().Be("INV-00001");
        result.Status.Should().Be(nameof(InvoiceStatus.Issued));
        result.Total.Should().Be(2700);
        result.OutstandingBalance.Should().Be(2700);
        result.Lines.Should().ContainSingle(l => l.ProductSku == "SKU-1" && l.Quantity == 3);
    }

    [Fact]
    public async Task Cannot_invoice_the_same_order_twice()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var orderId = await ConfirmedOrderAsync(db, 1, 100);
        var sut = new CreateInvoiceCommandHandler(db, new FakeTenantProvider(_tenantId));
        await sut.Handle(new CreateInvoiceCommand(orderId, null, null), CancellationToken.None);

        var act = () => sut.Handle(new CreateInvoiceCommand(orderId, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Registering_a_payment_persists_it_and_updates_status()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var orderId = await ConfirmedOrderAsync(db, 1, 1000);
        var invoice = await new CreateInvoiceCommandHandler(db, new FakeTenantProvider(_tenantId))
            .Handle(new CreateInvoiceCommand(orderId, null, null), CancellationToken.None);

        var afterPartial = await new RegisterPaymentCommandHandler(db).Handle(
            new RegisterPaymentCommand(invoice.Id, 400, Today, PaymentMethod.Cash, "TRF"), CancellationToken.None);

        afterPartial.Status.Should().Be(nameof(InvoiceStatus.PartiallyPaid));
        afterPartial.AmountPaid.Should().Be(400);
        afterPartial.Payments.Should().ContainSingle();

        // El pago quedó persistido (regresión del bug de fixup INSERT/UPDATE).
        db.Payments.Count(p => p.InvoiceId == invoice.Id).Should().Be(1);
    }

    [Fact]
    public async Task Voiding_an_invoice_with_payments_is_rejected()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var orderId = await ConfirmedOrderAsync(db, 1, 1000);
        var invoice = await new CreateInvoiceCommandHandler(db, new FakeTenantProvider(_tenantId))
            .Handle(new CreateInvoiceCommand(orderId, null, null), CancellationToken.None);
        await new RegisterPaymentCommandHandler(db).Handle(
            new RegisterPaymentCommand(invoice.Id, 100, Today, PaymentMethod.Cash, null), CancellationToken.None);

        var act = () => new VoidInvoiceCommandHandler(db).Handle(new VoidInvoiceCommand(invoice.Id), CancellationToken.None);

        await act.Should().ThrowAsync<Domain.Common.Exceptions.DomainException>();
    }
}
