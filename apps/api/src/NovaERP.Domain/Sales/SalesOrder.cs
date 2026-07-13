using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Sales;

/// <summary>
/// Pedido de venta: raíz del agregado que agrupa sus líneas. Protege su propia
/// máquina de estados (Draft → Confirmed → Cancelled). El movimiento real de
/// stock lo orquesta el Handler —que sí tiene acceso a los productos—, pero es
/// esta entidad quien decide qué transiciones son válidas.
/// </summary>
public sealed class SalesOrder : TenantAuditableEntity
{
    public string OrderNumber { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public DateOnly OrderDate { get; private set; }
    public string? Notes { get; private set; }
    public decimal TotalAmount { get; private set; }

    private readonly List<SalesOrderLine> _lines = [];
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();

    private SalesOrder() { }

    public SalesOrder(Guid tenantId, string orderNumber, Guid customerId, DateOnly orderDate, string? notes)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        OrderDate = orderDate;
        Notes = notes;
        Status = SalesOrderStatus.Draft;
    }

    public void AddLine(SalesOrderLine line)
    {
        if (Status != SalesOrderStatus.Draft)
        {
            throw new DomainException("Solo se pueden agregar líneas a un pedido en borrador.");
        }

        _lines.Add(line);
        TotalAmount += line.LineTotal;
    }

    /// <summary>
    /// Marca el pedido como confirmado. El Handler debe haber descontado el stock
    /// de cada línea antes de llamar aquí, en la misma transacción.
    /// </summary>
    public void Confirm()
    {
        if (Status != SalesOrderStatus.Draft)
        {
            throw new DomainException("Solo se puede confirmar un pedido en borrador.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainException("No se puede confirmar un pedido sin líneas.");
        }

        Status = SalesOrderStatus.Confirmed;
    }

    /// <summary>
    /// Cancela el pedido. Si estaba confirmado, el Handler debe devolver el stock
    /// de cada línea; esta entidad solo valida que la transición sea posible.
    /// </summary>
    public void Cancel()
    {
        if (Status == SalesOrderStatus.Cancelled)
        {
            throw new DomainException("El pedido ya está cancelado.");
        }

        Status = SalesOrderStatus.Cancelled;
    }
}
