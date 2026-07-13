using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.ListProducts;

public sealed class ListProductsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListProductsQuery, List<ProductSummary>>
{
    public async Task<List<ProductSummary>> Handle(ListProductsQuery request, CancellationToken ct)
    {
        var query = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Sku.ToLower().Contains(term));
        }

        if (request.CategoryId is { } categoryId)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (request.LowStockOnly)
        {
            query = query.Where(p => p.ReorderPoint != null && p.QuantityOnHand <= p.ReorderPoint);
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Id, p.Sku, p.Name, p.Description, p.CategoryId, p.UnitOfMeasure,
                p.CostPrice, p.SalePrice, p.QuantityOnHand, p.ReorderPoint, p.IsActive,
            })
            .ToListAsync(ct);

        var categoryNames = await db.ProductCategories.ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        return products.Select(p => new ProductSummary(
            p.Id, p.Sku, p.Name, p.Description, p.CategoryId,
            p.CategoryId is { } cid && categoryNames.TryGetValue(cid, out var name) ? name : null,
            p.UnitOfMeasure, p.CostPrice, p.SalePrice, p.QuantityOnHand, p.ReorderPoint, p.IsActive,
            p.ReorderPoint is { } reorder && p.QuantityOnHand <= reorder))
            .ToList();
    }
}
