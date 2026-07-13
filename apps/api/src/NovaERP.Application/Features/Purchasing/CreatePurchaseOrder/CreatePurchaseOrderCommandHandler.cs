using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Purchasing.Common;
using NovaERP.Domain.Partners;
using NovaERP.Domain.Purchasing;

namespace NovaERP.Application.Features.Purchasing.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderCommandHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDetail>
{
    public async Task<PurchaseOrderDetail> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        var supplier = await db.Partners.FirstOrDefaultAsync(p => p.Id == request.SupplierId, ct)
            ?? throw new ConflictException("El proveedor no existe.");

        if ((supplier.Type & PartnerType.Supplier) != PartnerType.Supplier)
        {
            throw new ConflictException("El contacto seleccionado no es un proveedor.");
        }

        if (!supplier.IsActive)
        {
            throw new ConflictException("El proveedor está inactivo.");
        }

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        var order = new PurchaseOrder(
            tenantProvider.TenantId,
            await GenerateOrderNumberAsync(ct),
            supplier.Id,
            request.OrderDate,
            request.Notes?.Trim());

        foreach (var input in request.Lines)
        {
            if (!products.TryGetValue(input.ProductId, out var product))
            {
                throw new ConflictException("Uno o más productos no existen.");
            }

            if (!product.IsActive)
            {
                throw new ConflictException($"El producto '{product.Name}' está inactivo.");
            }

            // Snapshot del SKU, nombre y costo vigente al momento de la compra.
            order.AddLine(new PurchaseOrderLine(
                product.Id, product.Sku, product.Name, input.Quantity, product.CostPrice));
        }

        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync(ct);

        return await PurchaseOrderDetailFactory.CreateAsync(db, order, ct);
    }

    /// <summary>Numeración correlativa por empresa (PO-00001); ver la nota de concurrencia en Ventas.</summary>
    private async Task<string> GenerateOrderNumberAsync(CancellationToken ct)
    {
        var count = await db.PurchaseOrders.CountAsync(ct);
        return $"PO-{count + 1:D5}";
    }
}
