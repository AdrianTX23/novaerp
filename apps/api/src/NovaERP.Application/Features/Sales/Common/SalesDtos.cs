namespace NovaERP.Application.Features.Sales.Common;

public sealed record SalesOrderLineDto(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

/// <summary>Vista de lista: lo justo para la tabla de pedidos, sin traer las líneas.</summary>
public sealed record SalesOrderSummary(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateOnly OrderDate,
    decimal TotalAmount,
    int LineCount);

/// <summary>Vista de detalle: el pedido completo con sus líneas.</summary>
public sealed record SalesOrderDetail(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateOnly OrderDate,
    string? Notes,
    decimal TotalAmount,
    IReadOnlyList<SalesOrderLineDto> Lines);
