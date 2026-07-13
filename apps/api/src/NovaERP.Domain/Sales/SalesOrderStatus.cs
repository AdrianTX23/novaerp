namespace NovaERP.Domain.Sales;

/// <summary>
/// Ciclo de vida de un pedido de venta. El stock solo se compromete en
/// Confirmed; Draft es una cotización que aún no reserva inventario.
/// </summary>
public enum SalesOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    Cancelled = 2,
}
