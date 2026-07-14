using NovaERP.Domain.Common;

namespace NovaERP.Domain.Invoicing;

/// <summary>
/// Línea de factura: snapshot de una línea del pedido de venta al momento de
/// emitir. Una factura es un documento legal congelado, así que no referencia
/// datos vivos del producto.
/// </summary>
public sealed class InvoiceLine : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public string ProductSku { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    private InvoiceLine() { }

    public InvoiceLine(string productSku, string productName, decimal quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ProductSku = productSku;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = quantity * unitPrice;
    }
}
