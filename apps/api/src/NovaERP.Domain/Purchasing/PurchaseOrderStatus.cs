namespace NovaERP.Domain.Purchasing;

/// <summary>
/// Ciclo de vida de una orden de compra. El stock solo entra al inventario en
/// Confirmed (recepción de mercancía); Draft es una orden aún no recibida.
/// </summary>
public enum PurchaseOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    Cancelled = 2,
}
