using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Purchasing.Common;

namespace NovaERP.Application.Features.Purchasing.ConfirmPurchaseOrder;

/// <summary>
/// Confirma una orden de compra y suma al inventario el stock recibido de cada
/// línea, en una sola transacción. Sumar stock nunca falla, así que —a diferencia
/// de Ventas— la confirmación de una compra no puede rechazarse por inventario.
/// </summary>
public sealed class ConfirmPurchaseOrderCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ConfirmPurchaseOrderCommand, PurchaseOrderDetail>
{
    public async Task<PurchaseOrderDetail> Handle(ConfirmPurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new ConflictException("La orden no existe.");

        var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        foreach (var line in order.Lines)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
            {
                throw new ConflictException("Uno o más productos de la orden ya no existen.");
            }

            // Recepción de mercancía: suma stock.
            product.AdjustStock(line.Quantity);
        }

        order.Confirm();
        await db.SaveChangesAsync(ct);

        return await PurchaseOrderDetailFactory.CreateAsync(db, order, ct);
    }
}
