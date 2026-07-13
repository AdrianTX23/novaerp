using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Sales;

/// <summary>
/// Línea de un pedido de venta. Guarda un snapshot del SKU, nombre y precio del
/// producto al momento de la venta: si el producto cambia de precio después, el
/// pedido histórico debe seguir reflejando lo que realmente se cobró. Mantiene
/// también ProductId para el movimiento de stock y la trazabilidad.
/// </summary>
public sealed class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductSku { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    /// <summary>Total de la línea = cantidad × precio unitario. Persistido para no recalcular en reportes.</summary>
    public decimal LineTotal { get; private set; }

    private SalesOrderLine() { }

    public SalesOrderLine(Guid productId, string productSku, string productName, decimal quantity, decimal unitPrice)
    {
        if (quantity <= 0)
        {
            throw new DomainException("La cantidad de una línea debe ser mayor que cero.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("El precio unitario no puede ser negativo.");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductSku = productSku;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = quantity * unitPrice;
    }
}
