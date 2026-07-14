using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Invoicing;

/// <summary>
/// Factura emitida desde un pedido de venta confirmado. Raíz del agregado:
/// agrupa sus líneas (snapshot) y sus pagos, y es la frontera de consistencia
/// del saldo (nadie puede pagar más de lo que se debe).
/// </summary>
public sealed class Invoice : TenantAuditableEntity
{
    public string InvoiceNumber { get; private set; } = null!;
    public Guid SalesOrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = null!;
    public InvoiceStatus Status { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal Total { get; private set; }
    public decimal AmountPaid { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<InvoiceLine> _lines = [];
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public decimal OutstandingBalance => Total - AmountPaid;

    private Invoice() { }

    public Invoice(
        Guid tenantId, string invoiceNumber, Guid salesOrderId, Guid customerId, string customerName,
        DateOnly issueDate, DateOnly dueDate, string? notes)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        SalesOrderId = salesOrderId;
        CustomerId = customerId;
        CustomerName = customerName;
        IssueDate = issueDate;
        DueDate = dueDate;
        Notes = notes;
        Status = InvoiceStatus.Issued;
    }

    public void AddLine(InvoiceLine line)
    {
        _lines.Add(line);
        Total += line.LineTotal;
    }

    /// <summary>
    /// Registra un abono. No permite pagar una factura anulada o ya pagada, ni
    /// un monto que exceda el saldo pendiente. Actualiza el estado según lo abonado
    /// y devuelve el pago creado: el Handler lo agrega vía su DbSet para que EF lo
    /// trate como INSERT (agregar un hijo a un agregado ya cargado confunde el
    /// fixup y genera un UPDATE de una fila inexistente).
    /// </summary>
    public Payment RegisterPayment(decimal amount, DateOnly paidAt, PaymentMethod method, string? reference)
    {
        if (Status == InvoiceStatus.Void)
        {
            throw new DomainException("No se puede pagar una factura anulada.");
        }

        if (Status == InvoiceStatus.Paid)
        {
            throw new DomainException("La factura ya está pagada por completo.");
        }

        if (amount <= 0)
        {
            throw new DomainException("El monto del pago debe ser mayor que cero.");
        }

        if (amount > OutstandingBalance)
        {
            throw new DomainException("El pago excede el saldo pendiente de la factura.");
        }

        var payment = new Payment(TenantId, Id, amount, paidAt, method, reference);
        _payments.Add(payment);
        AmountPaid += amount;
        Status = AmountPaid >= Total ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        return payment;
    }

    /// <summary>Anula la factura. Solo posible mientras no tenga pagos registrados.</summary>
    public void Void()
    {
        if (Status == InvoiceStatus.Void)
        {
            throw new DomainException("La factura ya está anulada.");
        }

        if (AmountPaid > 0)
        {
            throw new DomainException("No se puede anular una factura con pagos registrados.");
        }

        Status = InvoiceStatus.Void;
    }
}
