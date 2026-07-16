using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Sales.Common;
using NovaERP.Domain.Sales;

namespace NovaERP.Application.Features.Sales.ListSalesOrders;

public sealed class ListSalesOrdersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListSalesOrdersQuery, PagedResult<SalesOrderSummary>>
{
    public async Task<PagedResult<SalesOrderSummary>> Handle(ListSalesOrdersQuery request, CancellationToken ct)
    {
        var query = db.SalesOrders.AsQueryable();

        if (request.CustomerId is { } customerId)
        {
            query = query.Where(o => o.CustomerId == customerId);
        }

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<SalesOrderStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var totalCount = await query.CountAsync(ct);

        // Join contra Partners para el nombre del cliente; una sola consulta.
        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderNumber)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new SalesOrderSummary(
                o.Id,
                o.OrderNumber,
                o.CustomerId,
                db.Partners.Where(p => p.Id == o.CustomerId).Select(p => p.Name).FirstOrDefault() ?? "(cliente eliminado)",
                o.Status.ToString(),
                o.OrderDate,
                o.TotalAmount,
                o.Lines.Count))
            .ToListAsync(ct);

        return new PagedResult<SalesOrderSummary>(items, totalCount);
    }
}
