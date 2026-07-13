using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Domain.Catalog;

namespace NovaERP.Application.Features.Catalog.Common;

/// <summary>Arma un ProductSummary resolviendo el nombre de categoría; centraliza lo que
/// repetían Create/Update/AdjustStock/SetActive de Producto.</summary>
public static class ProductSummaryFactory
{
    public static async Task<ProductSummary> CreateAsync(IApplicationDbContext db, Product product, CancellationToken ct)
    {
        var categoryName = product.CategoryId is { } categoryId
            ? await db.ProductCategories.Where(c => c.Id == categoryId).Select(c => c.Name).FirstOrDefaultAsync(ct)
            : null;

        return new ProductSummary(
            product.Id, product.Sku, product.Name, product.Description, product.CategoryId, categoryName,
            product.UnitOfMeasure, product.CostPrice, product.SalePrice, product.QuantityOnHand,
            product.ReorderPoint, product.IsActive,
            product.ReorderPoint is { } reorder && product.QuantityOnHand <= reorder);
    }
}
