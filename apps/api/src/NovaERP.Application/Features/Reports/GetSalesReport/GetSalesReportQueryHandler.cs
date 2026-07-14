using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Reports.Common;
using NovaERP.Domain.Sales;

namespace NovaERP.Application.Features.Reports.GetSalesReport;

/// <summary>
/// Ventas confirmadas dentro de un rango de fechas, con desglose diario. A
/// diferencia del Dashboard (siempre "este mes"), aquí el rango lo elige el
/// usuario.
/// </summary>
public sealed class GetSalesReportQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken ct)
    {
        var orders = await db.SalesOrders
            .Where(o => o.Status == SalesOrderStatus.Confirmed && o.OrderDate >= request.From && o.OrderDate <= request.To)
            .Select(o => new { o.OrderDate, o.TotalAmount })
            .ToListAsync(ct);

        var totalSales = orders.Sum(o => o.TotalAmount);
        var orderCount = orders.Count;

        var daily = orders
            .GroupBy(o => o.OrderDate)
            .Select(g => new DailySalesPoint(g.Key, g.Sum(x => x.TotalAmount), g.Count()))
            .OrderBy(p => p.Date)
            .ToList();

        return new SalesReportDto(
            request.From, request.To, totalSales, orderCount,
            AverageOrderValue: orderCount > 0 ? totalSales / orderCount : 0,
            DailyBreakdown: daily);
    }
}
