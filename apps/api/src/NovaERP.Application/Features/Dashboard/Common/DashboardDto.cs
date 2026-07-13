namespace NovaERP.Application.Features.Dashboard.Common;

/// <summary>Un punto de la serie mensual (ej. Month = "2026-07").</summary>
public sealed record MonthlyPoint(string Month, decimal Total);

public sealed record TopProductDto(string ProductName, decimal QuantitySold, decimal Revenue);

/// <summary>
/// Instantánea de indicadores para el dashboard ejecutivo. Se calcula en una
/// sola query (el dashboard se carga como una unidad) y solo considera pedidos
/// confirmados: los borradores aún no son ventas/compras reales y los cancelados
/// están anulados.
/// </summary>
public sealed record DashboardDto(
    decimal SalesThisMonth,
    int SalesOrdersThisMonth,
    decimal PurchasesThisMonth,
    int LowStockCount,
    decimal InventoryValue,
    string? TopCustomerName,
    decimal TopCustomerRevenue,
    IReadOnlyList<MonthlyPoint> SalesTrend,
    IReadOnlyList<TopProductDto> TopProducts);
