using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.UpdateProduct;

public sealed class UpdateProductCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateProductCommand, ProductSummary>
{
    public async Task<ProductSummary> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new ConflictException("El producto no existe.");

        if (request.CategoryId is { } categoryId && !await db.ProductCategories.AnyAsync(c => c.Id == categoryId, ct))
        {
            throw new ConflictException("La categoría no existe.");
        }

        product.Update(
            request.Name, request.UnitOfMeasure, request.CostPrice, request.SalePrice,
            request.CategoryId, request.Description?.Trim(), request.ReorderPoint);

        await db.SaveChangesAsync(ct);

        return await ProductSummaryFactory.CreateAsync(db, product, ct);
    }
}
