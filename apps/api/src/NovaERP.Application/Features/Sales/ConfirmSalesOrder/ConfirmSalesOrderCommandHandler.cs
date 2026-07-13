using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Sales.Common;

namespace NovaERP.Application.Features.Sales.ConfirmSalesOrder;

/// <summary>
/// Confirma un pedido y descuenta el stock de cada línea en una sola
/// transacción. Si algún producto no tiene stock suficiente, Product.AdjustStock
/// lanza DomainException (→ 409) y el SaveChanges nunca llega a ejecutarse: no
/// queda ningún descuento a medias.
/// </summary>
public sealed class ConfirmSalesOrderCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ConfirmSalesOrderCommand, SalesOrderDetail>
{
    public async Task<SalesOrderDetail> Handle(ConfirmSalesOrderCommand request, CancellationToken ct)
    {
        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new ConflictException("El pedido no existe.");

        var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        foreach (var line in order.Lines)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
            {
                throw new ConflictException("Uno o más productos del pedido ya no existen.");
            }

            // Descuenta el stock; lanza DomainException si dejaría el saldo en negativo.
            product.AdjustStock(-line.Quantity);
        }

        order.Confirm();
        await db.SaveChangesAsync(ct);

        return await SalesOrderDetailFactory.CreateAsync(db, order, ct);
    }
}
