namespace NovaERP.Domain.Invoicing;

/// <summary>
/// Ciclo de vida de una factura. El estado de pago se deriva del monto abonado:
/// Issued (sin pagos) → PartiallyPaid → Paid. Void es una anulación (solo posible
/// mientras no haya pagos registrados).
/// </summary>
public enum InvoiceStatus
{
    Issued = 0,
    PartiallyPaid = 1,
    Paid = 2,
    Void = 3,
}
