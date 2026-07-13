using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Purchasing.Common;
using NovaERP.Domain.Purchasing;

namespace NovaERP.Application.Features.Purchasing.ListPurchaseOrders;

public sealed class ListPurchaseOrdersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListPurchaseOrdersQuery, List<PurchaseOrderSummary>>
{
    public async Task<List<PurchaseOrderSummary>> Handle(ListPurchaseOrdersQuery request, CancellationToken ct)
    {
        var query = db.PurchaseOrders.AsQueryable();

        if (request.SupplierId is { } supplierId)
        {
            query = query.Where(o => o.SupplierId == supplierId);
        }

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<PurchaseOrderStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderNumber)
            .Select(o => new PurchaseOrderSummary(
                o.Id,
                o.OrderNumber,
                o.SupplierId,
                db.Partners.Where(p => p.Id == o.SupplierId).Select(p => p.Name).FirstOrDefault() ?? "(proveedor eliminado)",
                o.Status.ToString(),
                o.OrderDate,
                o.TotalAmount,
                o.Lines.Count))
            .ToListAsync(ct);
    }
}
