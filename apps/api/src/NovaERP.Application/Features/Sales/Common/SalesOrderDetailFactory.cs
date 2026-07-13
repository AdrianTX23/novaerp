using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Domain.Sales;

namespace NovaERP.Application.Features.Sales.Common;

/// <summary>Arma un SalesOrderDetail resolviendo el nombre del cliente; centraliza lo que
/// repetirían Create/Confirm/Cancel/Get.</summary>
public static class SalesOrderDetailFactory
{
    public static async Task<SalesOrderDetail> CreateAsync(IApplicationDbContext db, SalesOrder order, CancellationToken ct)
    {
        var customerName = await db.Partners
            .Where(p => p.Id == order.CustomerId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(ct) ?? "(cliente eliminado)";

        var lines = order.Lines
            .Select(l => new SalesOrderLineDto(
                l.ProductId, l.ProductSku, l.ProductName, l.Quantity, l.UnitPrice, l.LineTotal))
            .ToList();

        return new SalesOrderDetail(
            order.Id, order.OrderNumber, order.CustomerId, customerName,
            order.Status.ToString(), order.OrderDate, order.Notes, order.TotalAmount, lines);
    }
}
