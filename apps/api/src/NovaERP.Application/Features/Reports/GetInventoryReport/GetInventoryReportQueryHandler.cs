using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Reports.Common;

namespace NovaERP.Application.Features.Reports.GetInventoryReport;

public sealed class GetInventoryReportQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventoryReportQuery, InventoryReportDto>
{
    public async Task<InventoryReportDto> Handle(GetInventoryReportQuery request, CancellationToken ct)
    {
        var products = await db.Products
            .Where(p => p.IsActive)
            .Select(p => new
            {
                p.Sku,
                p.Name,
                p.QuantityOnHand,
                p.CostPrice,
                p.ReorderPoint,
                CategoryName = p.CategoryId == null
                    ? "Sin categoría"
                    : db.ProductCategories.Where(c => c.Id == p.CategoryId).Select(c => c.Name).FirstOrDefault(),
            })
            .ToListAsync(ct);

        var byCategory = products
            .GroupBy(p => p.CategoryName ?? "Sin categoría")
            .Select(g => new CategoryValuationRow(g.Key, g.Count(), g.Sum(p => p.QuantityOnHand * p.CostPrice)))
            .OrderByDescending(r => r.Value)
            .ToList();

        var lowStock = products
            .Where(p => p.ReorderPoint != null && p.QuantityOnHand <= p.ReorderPoint)
            .Select(p => new LowStockRow(p.Sku, p.Name, p.QuantityOnHand, p.ReorderPoint!.Value))
            .OrderBy(r => r.QuantityOnHand)
            .ToList();

        return new InventoryReportDto(
            TotalValue: products.Sum(p => p.QuantityOnHand * p.CostPrice),
            TotalProducts: products.Count,
            ByCategory: byCategory,
            LowStock: lowStock);
    }
}
