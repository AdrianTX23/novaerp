using FluentAssertions;
using NovaERP.Domain.Common.Exceptions;
using NovaERP.Domain.Invoicing;

namespace NovaERP.UnitTests.Invoicing;

public sealed class InvoiceTests
{
    private static Invoice NewInvoice(decimal total = 1000)
    {
        var invoice = new Invoice(
            Guid.NewGuid(), "INV-00001", Guid.NewGuid(), Guid.NewGuid(), "ACME",
            new DateOnly(2026, 7, 14), new DateOnly(2026, 8, 13), null);
        invoice.AddLine(new InvoiceLine("SKU-1", "Widget", 1, total));
        return invoice;
    }

    private static readonly DateOnly PayDate = new(2026, 7, 14);

    [Fact]
    public void New_invoice_is_issued_with_full_balance()
    {
        var invoice = NewInvoice(1000);

        invoice.Status.Should().Be(InvoiceStatus.Issued);
        invoice.AmountPaid.Should().Be(0);
        invoice.OutstandingBalance.Should().Be(1000);
    }

    [Fact]
    public void Partial_payment_moves_to_partially_paid()
    {
        var invoice = NewInvoice(1000);

        invoice.RegisterPayment(400, PayDate, PaymentMethod.Cash, null);

        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        invoice.OutstandingBalance.Should().Be(600);
    }

    [Fact]
    public void Full_payment_across_installments_moves_to_paid()
    {
        var invoice = NewInvoice(1000);

        invoice.RegisterPayment(400, PayDate, PaymentMethod.Cash, null);
        invoice.RegisterPayment(600, PayDate, PaymentMethod.Transfer, null);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.OutstandingBalance.Should().Be(0);
        invoice.Payments.Should().HaveCount(2);
    }

    [Fact]
    public void Payment_exceeding_the_balance_is_rejected()
    {
        var invoice = NewInvoice(1000);

        var act = () => invoice.RegisterPayment(1500, PayDate, PaymentMethod.Cash, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cannot_pay_a_fully_paid_invoice()
    {
        var invoice = NewInvoice(1000);
        invoice.RegisterPayment(1000, PayDate, PaymentMethod.Cash, null);

        var act = () => invoice.RegisterPayment(1, PayDate, PaymentMethod.Cash, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Can_void_an_invoice_without_payments()
    {
        var invoice = NewInvoice(1000);

        invoice.Void();

        invoice.Status.Should().Be(InvoiceStatus.Void);
    }

    [Fact]
    public void Cannot_void_an_invoice_with_payments()
    {
        var invoice = NewInvoice(1000);
        invoice.RegisterPayment(100, PayDate, PaymentMethod.Cash, null);

        var act = invoice.Void;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cannot_pay_a_voided_invoice()
    {
        var invoice = NewInvoice(1000);
        invoice.Void();

        var act = () => invoice.RegisterPayment(100, PayDate, PaymentMethod.Cash, null);

        act.Should().Throw<DomainException>();
    }
}
