using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Purchasing.Common;
using NovaERP.Domain.Purchasing;

namespace NovaERP.Application.Features.Purchasing.CancelPurchaseOrder;

/// <summary>
/// Cancela una orden de compra. Si estaba confirmada, revierte el stock que se
/// había recibido. Esa reversión PUEDE fallar: si el stock recibido ya se
/// consumió (p. ej. se vendió), Product.AdjustStock lanza DomainException (→ 409)
/// y la cancelación se rechaza sin dejar el inventario en negativo.
/// </summary>
public sealed class CancelPurchaseOrderCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CancelPurchaseOrderCommand, PurchaseOrderDetail>
{
    public async Task<PurchaseOrderDetail> Handle(CancelPurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new ConflictException("La orden no existe.");

        var wasConfirmed = order.Status == PurchaseOrderStatus.Confirmed;

        // Valida la transición (lanza si ya estaba cancelada) antes de tocar stock.
        order.Cancel();

        if (wasConfirmed)
        {
            var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

            foreach (var line in order.Lines)
            {
                if (products.TryGetValue(line.ProductId, out var product))
                {
                    // Revierte la recepción; lanza si el stock ya se consumió.
                    product.AdjustStock(-line.Quantity);
                }
            }
        }

        await db.SaveChangesAsync(ct);

        return await PurchaseOrderDetailFactory.CreateAsync(db, order, ct);
    }
}
