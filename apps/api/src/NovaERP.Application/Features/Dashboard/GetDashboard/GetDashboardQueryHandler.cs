using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Dashboard.Common;
using NovaERP.Domain.Purchasing;
using NovaERP.Domain.Sales;

namespace NovaERP.Application.Features.Dashboard.GetDashboard;

public sealed class GetDashboardQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private const int TrendMonths = 6;

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var trendStart = monthStart.AddMonths(-(TrendMonths - 1));

        var confirmedSales = db.SalesOrders.Where(o => o.Status == SalesOrderStatus.Confirmed);
        var confirmedPurchases = db.PurchaseOrders.Where(o => o.Status == PurchaseOrderStatus.Confirmed);

        var salesThisMonth = await confirmedSales
            .Where(o => o.OrderDate >= monthStart && o.OrderDate < nextMonthStart)
            .SumAsync(o => o.TotalAmount, ct);

        var salesOrdersThisMonth = await confirmedSales
            .CountAsync(o => o.OrderDate >= monthStart && o.OrderDate < nextMonthStart, ct);

        var purchasesThisMonth = await confirmedPurchases
            .Where(o => o.OrderDate >= monthStart && o.OrderDate < nextMonthStart)
            .SumAsync(o => o.TotalAmount, ct);

        var lowStockCount = await db.Products
            .CountAsync(p => p.IsActive && p.ReorderPoint != null && p.QuantityOnHand <= p.ReorderPoint, ct);

        var inventoryValue = await db.Products
            .Where(p => p.IsActive)
            .SumAsync(p => p.QuantityOnHand * p.CostPrice, ct);

        // Mejor cliente por ingresos confirmados (histórico).
        var topCustomer = await confirmedSales
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, Revenue = g.Sum(x => x.TotalAmount) })
            .OrderByDescending(x => x.Revenue)
            .FirstOrDefaultAsync(ct);

        string? topCustomerName = null;
        decimal topCustomerRevenue = 0;
        if (topCustomer is not null)
        {
            topCustomerName = await db.Partners
                .Where(p => p.Id == topCustomer.CustomerId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(ct);
            topCustomerRevenue = topCustomer.Revenue;
        }

        // Serie de ventas de los últimos 6 meses: una query agrupada + relleno de huecos en C#.
        var monthly = await confirmedSales
            .Where(o => o.OrderDate >= trendStart)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.TotalAmount) })
            .ToListAsync(ct);

        var salesTrend = Enumerable.Range(0, TrendMonths)
            .Select(i => trendStart.AddMonths(i))
            .Select(m => new MonthlyPoint(
                $"{m.Year:D4}-{m.Month:D2}",
                monthly.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Total ?? 0))
            .ToList();

        // Top productos: Npgsql no traduce un GroupBy sobre un Contains de subquery,
        // así que se materializan las líneas de pedidos confirmados (volumen acotado)
        // y se agrupan en memoria.
        var confirmedSalesIds = await confirmedSales.Select(o => o.Id).ToListAsync(ct);
        var confirmedLines = await db.SalesOrderLines
            .Where(l => confirmedSalesIds.Contains(l.SalesOrderId))
            .Select(l => new { l.ProductName, l.Quantity, l.LineTotal })
            .ToListAsync(ct);

        var topProducts = confirmedLines
            .GroupBy(l => l.ProductName)
            .Select(g => new TopProductDto(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.LineTotal)))
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToList();

        return new DashboardDto(
            salesThisMonth, salesOrdersThisMonth, purchasesThisMonth, lowStockCount, inventoryValue,
            topCustomerName, topCustomerRevenue, salesTrend, topProducts);
    }
}
