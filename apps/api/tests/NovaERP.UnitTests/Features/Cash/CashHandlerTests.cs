using FluentAssertions;
using NovaERP.Application.Features.Cash.CreateCashMovement;
using NovaERP.Application.Features.Cash.DeleteCashMovement;
using NovaERP.Application.Features.Cash.GetCashSummary;
using NovaERP.Application.Features.Cash.ListCashMovements;
using NovaERP.Domain.Cash;
using NovaERP.Domain.Invoicing;
using NovaERP.Infrastructure.Persistence;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Cash;

public sealed class CashHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Siembra una factura con un pago (ingreso) directo en el contexto.</summary>
    private async Task SeedInvoicePaymentAsync(NovaErpDbContext db, decimal amount)
    {
        var invoice = new Invoice(_tenantId, "INV-00001", Guid.NewGuid(), Guid.NewGuid(), "ACME", Today, Today, null);
        invoice.AddLine(new InvoiceLine("SKU-1", "Widget", 1, amount));
        var payment = invoice.RegisterPayment(amount, Today, PaymentMethod.Transfer, "TRF");
        db.Invoices.Add(invoice);
        db.Payments.Add(payment);
        await db.SaveChangesAsync(CancellationToken.None);
    }

    private CreateCashMovementCommand ManualMovement(CashMovementType type, decimal amount, string concept) =>
        new(type, amount, Today, concept, null);

    [Fact]
    public async Task Summary_combines_invoice_payments_and_manual_movements()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        await SeedInvoicePaymentAsync(db, 1500);
        var create = new CreateCashMovementCommandHandler(db, new FakeTenantProvider(_tenantId));
        await create.Handle(ManualMovement(CashMovementType.Income, 1000, "Aporte"), CancellationToken.None);
        await create.Handle(ManualMovement(CashMovementType.Expense, 400, "Renta"), CancellationToken.None);

        var summary = await new GetCashSummaryQueryHandler(db).Handle(new GetCashSummaryQuery(), CancellationToken.None);

        summary.TotalIncome.Should().Be(2500);  // 1500 pago + 1000 aporte
        summary.TotalExpense.Should().Be(400);
        summary.Balance.Should().Be(2100);
    }

    [Fact]
    public async Task Movements_list_unifies_payment_income_and_manual_entries()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        await SeedInvoicePaymentAsync(db, 1500);
        await new CreateCashMovementCommandHandler(db, new FakeTenantProvider(_tenantId))
            .Handle(ManualMovement(CashMovementType.Expense, 400, "Renta"), CancellationToken.None);

        var movements = await new ListCashMovementsQueryHandler(db)
            .Handle(new ListCashMovementsQuery(), CancellationToken.None);

        movements.Should().HaveCount(2);
        movements.Should().ContainSingle(m => m.Source == "Invoice" && m.Kind == "Income" && m.CanDelete == false);
        movements.Should().ContainSingle(m => m.Source == "Manual" && m.Kind == "Expense" && m.CanDelete);
    }

    [Fact]
    public async Task Deleting_a_manual_movement_updates_the_balance()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var created = await new CreateCashMovementCommandHandler(db, new FakeTenantProvider(_tenantId))
            .Handle(ManualMovement(CashMovementType.Income, 1000, "Aporte"), CancellationToken.None);

        await new DeleteCashMovementCommandHandler(db)
            .Handle(new DeleteCashMovementCommand(created.Id), CancellationToken.None);

        var summary = await new GetCashSummaryQueryHandler(db).Handle(new GetCashSummaryQuery(), CancellationToken.None);
        summary.Balance.Should().Be(0);
    }
}
