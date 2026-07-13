using NovaERP.Domain.Catalog;
using NovaERP.Domain.Partners;
using NovaERP.Infrastructure.Persistence;

namespace NovaERP.UnitTests.Features.Purchasing;

/// <summary>Siembra un proveedor y un producto para los tests de Compras.</summary>
internal static class PurchasingTestData
{
    public static async Task<(Partner supplier, Product product)> SeedAsync(
        NovaErpDbContext db, Guid tenantId, decimal initialStock = 0, decimal costPrice = 5m)
    {
        var supplier = new Partner(tenantId, "Proveedor Uno", PartnerType.Supplier);
        var product = new Product(tenantId, "SKU-1", "Insumo", "unidad", costPrice: costPrice, salePrice: 12m);
        if (initialStock > 0)
        {
            product.AdjustStock(initialStock);
        }

        db.Partners.Add(supplier);
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None);

        return (supplier, product);
    }
}
