using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Domain.Purchasing;

namespace NovaERP.Application.Features.Purchasing.Common;

/// <summary>Arma un PurchaseOrderDetail resolviendo el nombre del proveedor; centraliza lo que
/// repetirían Create/Confirm/Cancel/Get.</summary>
public static class PurchaseOrderDetailFactory
{
    public static async Task<PurchaseOrderDetail> CreateAsync(IApplicationDbContext db, PurchaseOrder order, CancellationToken ct)
    {
        var supplierName = await db.Partners
            .Where(p => p.Id == order.SupplierId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(ct) ?? "(proveedor eliminado)";

        var lines = order.Lines
            .Select(l => new PurchaseOrderLineDto(
                l.ProductId, l.ProductSku, l.ProductName, l.Quantity, l.UnitCost, l.LineTotal))
            .ToList();

        return new PurchaseOrderDetail(
            order.Id, order.OrderNumber, order.SupplierId, supplierName,
            order.Status.ToString(), order.OrderDate, order.Notes, order.TotalAmount, lines);
    }
}
