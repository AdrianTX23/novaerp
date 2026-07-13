using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Purchasing;

/// <summary>
/// Línea de una orden de compra. Guarda un snapshot del SKU, nombre y costo
/// unitario del producto al momento de la compra (lo que se pagó al proveedor),
/// más el ProductId para el movimiento de stock y la trazabilidad.
/// </summary>
public sealed class PurchaseOrderLine : BaseEntity
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductSku { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }

    /// <summary>Total de la línea = cantidad × costo unitario. Persistido para no recalcular en reportes.</summary>
    public decimal LineTotal { get; private set; }

    private PurchaseOrderLine() { }

    public PurchaseOrderLine(Guid productId, string productSku, string productName, decimal quantity, decimal unitCost)
    {
        if (quantity <= 0)
        {
            throw new DomainException("La cantidad de una línea debe ser mayor que cero.");
        }

        if (unitCost < 0)
        {
            throw new DomainException("El costo unitario no puede ser negativo.");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductSku = productSku;
        ProductName = productName;
        Quantity = quantity;
        UnitCost = unitCost;
        LineTotal = quantity * unitCost;
    }
}
