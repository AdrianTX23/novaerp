using NovaERP.Domain.Common;

namespace NovaERP.Domain.Invoicing;

/// <summary>Método por el que se recibió un pago.</summary>
public enum PaymentMethod
{
    Cash = 0,
    Transfer = 1,
    Card = 2,
    Other = 3,
}

/// <summary>
/// Pago registrado contra una factura. Vive dentro del agregado Invoice (la
/// factura es la frontera de consistencia del saldo), pero se expone también
/// como tabla propia para que la Caja (Fase 10) lea los ingresos directamente.
/// </summary>
public sealed class Payment : TenantAuditableEntity
{
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaidAt { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? Reference { get; private set; }

    private Payment() { }

    public Payment(Guid tenantId, Guid invoiceId, decimal amount, DateOnly paidAt, PaymentMethod method, string? reference)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        InvoiceId = invoiceId;
        Amount = amount;
        PaidAt = paidAt;
        Method = method;
        Reference = reference;
    }
}
