using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Sales.Common;
using NovaERP.Domain.Sales;

namespace NovaERP.Application.Features.Sales.CancelSalesOrder;

/// <summary>
/// Cancela un pedido. Si estaba confirmado, devuelve al inventario el stock que
/// se había descontado; si era un borrador, no había stock comprometido y solo
/// cambia el estado.
/// </summary>
public sealed class CancelSalesOrderCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CancelSalesOrderCommand, SalesOrderDetail>
{
    public async Task<SalesOrderDetail> Handle(CancelSalesOrderCommand request, CancellationToken ct)
    {
        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new ConflictException("El pedido no existe.");

        var wasConfirmed = order.Status == SalesOrderStatus.Confirmed;

        // Valida la transición (lanza si ya estaba cancelado) antes de tocar stock.
        order.Cancel();

        if (wasConfirmed)
        {
            var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

            foreach (var line in order.Lines)
            {
                if (products.TryGetValue(line.ProductId, out var product))
                {
                    product.AdjustStock(line.Quantity);
                }
            }
        }

        await db.SaveChangesAsync(ct);

        return await SalesOrderDetailFactory.CreateAsync(db, order, ct);
    }
}
