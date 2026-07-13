using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Catalog.Common;
using NovaERP.Domain.Catalog;

namespace NovaERP.Application.Features.Catalog.CreateProduct;

public sealed class CreateProductCommandHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    : IRequestHandler<CreateProductCommand, ProductSummary>
{
    public async Task<ProductSummary> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var sku = request.Sku.Trim();
        if (await db.Products.AnyAsync(p => p.Sku == sku, ct))
        {
            throw new ConflictException("Ya existe un producto con ese SKU.");
        }

        if (request.CategoryId is { } categoryId && !await db.ProductCategories.AnyAsync(c => c.Id == categoryId, ct))
        {
            throw new ConflictException("La categoría no existe.");
        }

        var product = new Product(
            tenantProvider.TenantId, sku, request.Name, request.UnitOfMeasure,
            request.CostPrice, request.SalePrice, request.CategoryId, request.Description?.Trim(),
            request.ReorderPoint);

        if (request.InitialQuantity > 0)
        {
            product.AdjustStock(request.InitialQuantity);
        }

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        return await ProductSummaryFactory.CreateAsync(db, product, ct);
    }
}
