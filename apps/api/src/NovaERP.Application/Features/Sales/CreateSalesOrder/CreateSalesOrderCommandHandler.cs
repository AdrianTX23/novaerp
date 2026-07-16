using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Sales.Common;
using NovaERP.Domain.Partners;
using NovaERP.Domain.Sales;

namespace NovaERP.Application.Features.Sales.CreateSalesOrder;

public sealed class CreateSalesOrderCommandHandler(
    IApplicationDbContext db, ITenantProvider tenantProvider, IDocumentSequenceService sequences)
    : IRequestHandler<CreateSalesOrderCommand, SalesOrderDetail>
{
    public async Task<SalesOrderDetail> Handle(CreateSalesOrderCommand request, CancellationToken ct)
    {
        var customer = await db.Partners.FirstOrDefaultAsync(p => p.Id == request.CustomerId, ct)
            ?? throw new ConflictException("El cliente no existe.");

        if ((customer.Type & PartnerType.Customer) != PartnerType.Customer)
        {
            throw new ConflictException("El contacto seleccionado no es un cliente.");
        }

        if (!customer.IsActive)
        {
            throw new ConflictException("El cliente está inactivo.");
        }

        // Trae todos los productos de las líneas de una sola consulta.
        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        var order = new SalesOrder(
            tenantProvider.TenantId,
            await GenerateOrderNumberAsync(ct),
            customer.Id,
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

            // Snapshot del SKU, nombre y precio de venta vigente al momento de la venta.
            order.AddLine(new SalesOrderLine(
                product.Id, product.Sku, product.Name, input.Quantity, product.SalePrice));
        }

        db.SalesOrders.Add(order);
        await db.SaveChangesAsync(ct);

        return await SalesOrderDetailFactory.CreateAsync(db, order, ct);
    }

    /// <summary>Numeración correlativa por empresa (SO-00001), atómica vía IDocumentSequenceService.</summary>
    private async Task<string> GenerateOrderNumberAsync(CancellationToken ct)
    {
        var next = await sequences.NextAsync(tenantProvider.TenantId, "SalesOrder", ct);
        return $"SO-{next:D5}";
    }
}
