using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Catalog;

public sealed class Product : TenantAuditableEntity
{
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string UnitOfMeasure { get; private set; } = null!;
    public decimal CostPrice { get; private set; }
    public decimal SalePrice { get; private set; }
    public decimal QuantityOnHand { get; private set; }

    /// <summary>Umbral bajo el cual el producto se considera de baja rotación / a reordenar.</summary>
    public decimal? ReorderPoint { get; private set; }

    public bool IsActive { get; private set; } = true;

    private Product() { }

    public Product(
        Guid tenantId,
        string sku,
        string name,
        string unitOfMeasure,
        decimal costPrice,
        decimal salePrice,
        Guid? categoryId = null,
        string? description = null,
        decimal? reorderPoint = null)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Sku = sku.Trim();
        Name = name.Trim();
        UnitOfMeasure = unitOfMeasure.Trim();
        CostPrice = costPrice;
        SalePrice = salePrice;
        CategoryId = categoryId;
        Description = description;
        ReorderPoint = reorderPoint;
    }

    public void Update(
        string name,
        string unitOfMeasure,
        decimal costPrice,
        decimal salePrice,
        Guid? categoryId,
        string? description,
        decimal? reorderPoint)
    {
        Name = name.Trim();
        UnitOfMeasure = unitOfMeasure.Trim();
        CostPrice = costPrice;
        SalePrice = salePrice;
        CategoryId = categoryId;
        Description = description;
        ReorderPoint = reorderPoint;
    }

    /// <summary>
    /// Ajuste manual de stock (entrada/salida/corrección). Cuando existan
    /// Compras y Ventas, esas transacciones llamarán a este mismo método desde
    /// su propio Handler — el ledger de movimientos vive en esos módulos, no
    /// aquí, porque son ellos quienes tienen el motivo de negocio del cambio.
    /// </summary>
    public void AdjustStock(decimal delta)
    {
        var newQuantity = QuantityOnHand + delta;
        if (newQuantity < 0)
        {
            throw new DomainException("El ajuste dejaría el stock en negativo.");
        }

        QuantityOnHand = newQuantity;
    }

    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;
}
