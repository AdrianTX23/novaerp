namespace NovaERP.Application.Features.Purchasing.Common;

public sealed record PurchaseOrderLineDto(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal Quantity,
    decimal UnitCost,
    decimal LineTotal);

/// <summary>Vista de lista: lo justo para la tabla de órdenes, sin traer las líneas.</summary>
public sealed record PurchaseOrderSummary(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    string Status,
    DateOnly OrderDate,
    decimal TotalAmount,
    int LineCount);

/// <summary>Vista de detalle: la orden completa con sus líneas.</summary>
public sealed record PurchaseOrderDetail(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    string Status,
    DateOnly OrderDate,
    string? Notes,
    decimal TotalAmount,
    IReadOnlyList<PurchaseOrderLineDto> Lines);
