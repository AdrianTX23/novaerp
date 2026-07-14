using FluentAssertions;
using NovaERP.Application.Features.Reports.GetInventoryReport;
using NovaERP.Application.Features.Reports.GetReceivablesReport;
using NovaERP.Application.Features.Reports.GetSalesReport;
using NovaERP.Application.Features.Sales.ConfirmSalesOrder;
using NovaERP.Application.Features.Sales.CreateSalesOrder;
using NovaERP.Domain.Catalog;
using NovaERP.Domain.Invoicing;
using NovaERP.Domain.Partners;
using NovaERP.Infrastructure.Persistence;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Reports;

public sealed class ReportsHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private async Task<Guid> ConfirmedSaleAsync(NovaErpDbContext db, Guid customerId, Guid productId, decimal qty, DateOnly date)
    {
        var order = await new CreateSalesOrderCommandHandler(db, new FakeTenantProvider(_tenantId)).Handle(
            new CreateSalesOrderCommand(customerId, date, null, [new CreateSalesOrderLineInput(productId, qty)]),
            CancellationToken.None);
        await new ConfirmSalesOrderCommandHandler(db).Handle(new ConfirmSalesOrderCommand(order.Id), CancellationToken.None);
        return order.Id;
    }

    [Fact]
    public async Task Sales_report_only_counts_confirmed_orders_within_range()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var customer = new Partner(_tenantId, "ACME", PartnerType.Customer);
        var product = new Product(_tenantId, "A", "Widget", "u", 5, 100);
        product.AdjustStock(100);
        db.Partners.Add(customer);
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None);

        await ConfirmedSaleAsync(db, customer.Id, product.Id, 2, Today); // dentro del rango: 200
        await ConfirmedSaleAsync(db, customer.Id, product.Id, 1, Today.AddDays(-90)); // fuera del rango

        var create = new CreateSalesOrderCommandHandler(db, new FakeTenantProvider(_tenantId));
        await create.Handle(new CreateSalesOrderCommand(customer.Id, Today, null,
            [new CreateSalesOrderLineInput(product.Id, 1)]), CancellationToken.None); // borrador, no confirmado

        var report = await new GetSalesReportQueryHandler(db)
            .Handle(new GetSalesReportQuery(Today.AddDays(-7), Today), CancellationToken.None);

        report.TotalSales.Should().Be(200);
        report.OrderCount.Should().Be(1);
        report.AverageOrderValue.Should().Be(200);
    }

    [Fact]
    public async Task Inventory_report_computes_valuation_and_low_stock()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var cheap = new Product(_tenantId, "A", "Barato", "u", 10, 20, reorderPoint: 5);
        cheap.AdjustStock(3); // bajo stock
        var normal = new Product(_tenantId, "B", "Normal", "u", 50, 90, reorderPoint: 5);
        normal.AdjustStock(20);
        db.Products.AddRange(cheap, normal);
        await db.SaveChangesAsync(CancellationToken.None);

        var report = await new GetInventoryReportQueryHandler(db).Handle(new GetInventoryReportQuery(), CancellationToken.None);

        report.TotalValue.Should().Be(3 * 10 + 20 * 50);
        report.TotalProducts.Should().Be(2);
        report.LowStock.Should().ContainSingle(l => l.Sku == "A");
    }

    [Fact]
    public async Task Receivables_report_excludes_paid_and_buckets_by_days_overdue()
    {
        using var db = TestDbContextFactory.Create(_tenantId);

        var current = new Invoice(_tenantId, "INV-1", Guid.NewGuid(), Guid.NewGuid(), "ACME", Today, Today.AddDays(10), null);
        current.AddLine(new InvoiceLine("A", "Widget", 1, 500));

        var overdue = new Invoice(_tenantId, "INV-2", Guid.NewGuid(), Guid.NewGuid(), "Globex", Today, Today.AddDays(-45), null);
        overdue.AddLine(new InvoiceLine("A", "Widget", 1, 900));

        var paid = new Invoice(_tenantId, "INV-3", Guid.NewGuid(), Guid.NewGuid(), "Initech", Today, Today.AddDays(-5), null);
        paid.AddLine(new InvoiceLine("A", "Widget", 1, 300));
        var payment = paid.RegisterPayment(300, Today, PaymentMethod.Cash, null);

        db.Invoices.AddRange(current, overdue, paid);
        db.Payments.Add(payment);
        await db.SaveChangesAsync(CancellationToken.None);

        var report = await new GetReceivablesReportQueryHandler(db)
            .Handle(new GetReceivablesReportQuery(), CancellationToken.None);

        report.TotalOutstanding.Should().Be(1400); // 500 + 900; la pagada no cuenta
        report.Invoices.Should().HaveCount(2);
        report.Invoices.Should().ContainSingle(r => r.InvoiceNumber == "INV-1" && r.Bucket == "Al día");
        report.Invoices.Should().ContainSingle(r => r.InvoiceNumber == "INV-2" && r.Bucket == "31-60 días");
    }
}
