namespace NovaERP.Application.Features.Reports.Common;

// --- Ventas ---

public sealed record DailySalesPoint(DateOnly Date, decimal Total, int OrderCount);

public sealed record SalesReportDto(
    DateOnly From,
    DateOnly To,
    decimal TotalSales,
    int OrderCount,
    decimal AverageOrderValue,
    IReadOnlyList<DailySalesPoint> DailyBreakdown);

// --- Inventario ---

public sealed record CategoryValuationRow(string CategoryName, int ProductCount, decimal Value);

public sealed record LowStockRow(string Sku, string Name, decimal QuantityOnHand, decimal ReorderPoint);

public sealed record InventoryReportDto(
    decimal TotalValue,
    int TotalProducts,
    IReadOnlyList<CategoryValuationRow> ByCategory,
    IReadOnlyList<LowStockRow> LowStock);

// --- Cuentas por cobrar ---

public sealed record ReceivableRow(
    Guid InvoiceId,
    string InvoiceNumber,
    string CustomerName,
    DateOnly DueDate,
    decimal OutstandingBalance,
    int DaysOverdue,
    string Bucket);

public sealed record AgingBucketTotal(string Bucket, decimal Total, int Count);

public sealed record ReceivablesReportDto(
    decimal TotalOutstanding,
    IReadOnlyList<AgingBucketTotal> Buckets,
    IReadOnlyList<ReceivableRow> Invoices);
