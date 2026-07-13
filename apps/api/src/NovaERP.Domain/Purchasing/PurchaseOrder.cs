using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Purchasing;

/// <summary>
/// Orden de compra: raíz del agregado que agrupa sus líneas. Protege su máquina
/// de estados (Draft → Confirmed → Cancelled). El movimiento real de stock lo
/// orquesta el Handler —que tiene acceso a los productos—, pero es esta entidad
/// quien decide qué transiciones son válidas.
/// </summary>
public sealed class PurchaseOrder : TenantAuditableEntity
{
    public string OrderNumber { get; private set; } = null!;
    public Guid SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateOnly OrderDate { get; private set; }
    public string? Notes { get; private set; }
    public decimal TotalAmount { get; private set; }

    private readonly List<PurchaseOrderLine> _lines = [];
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder() { }

    public PurchaseOrder(Guid tenantId, string orderNumber, Guid supplierId, DateOnly orderDate, string? notes)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        OrderNumber = orderNumber;
        SupplierId = supplierId;
        OrderDate = orderDate;
        Notes = notes;
        Status = PurchaseOrderStatus.Draft;
    }

    public void AddLine(PurchaseOrderLine line)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new DomainException("Solo se pueden agregar líneas a una orden en borrador.");
        }

        _lines.Add(line);
        TotalAmount += line.LineTotal;
    }

    /// <summary>
    /// Marca la orden como confirmada. El Handler debe haber sumado el stock de
    /// cada línea antes de llamar aquí, en la misma transacción.
    /// </summary>
    public void Confirm()
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new DomainException("Solo se puede confirmar una orden en borrador.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainException("No se puede confirmar una orden sin líneas.");
        }

        Status = PurchaseOrderStatus.Confirmed;
    }

    /// <summary>
    /// Cancela la orden. Si estaba confirmada, el Handler debe revertir el stock
    /// recibido; esta entidad solo valida que la transición sea posible.
    /// </summary>
    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Cancelled)
        {
            throw new DomainException("La orden ya está cancelada.");
        }

        Status = PurchaseOrderStatus.Cancelled;
    }
}
