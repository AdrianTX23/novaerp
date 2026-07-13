using NovaERP.Domain.Catalog;
using NovaERP.Domain.Partners;
using NovaERP.Infrastructure.Persistence;

namespace NovaERP.UnitTests.Features.Sales;

/// <summary>Siembra un cliente y un producto con stock para los tests de Ventas.</summary>
internal static class SalesTestData
{
    public static async Task<(Partner customer, Product product)> SeedAsync(
        NovaErpDbContext db, Guid tenantId, decimal initialStock = 10, decimal salePrice = 12.5m)
    {
        var customer = new Partner(tenantId, "Cliente Uno", PartnerType.Customer);
        var product = new Product(tenantId, "SKU-1", "Widget", "unidad", costPrice: 5, salePrice: salePrice);
        product.AdjustStock(initialStock);

        db.Partners.Add(customer);
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None);

        return (customer, product);
    }
}
