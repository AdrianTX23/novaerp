namespace NovaERP.API.Contracts;

public sealed record CreatePurchaseOrderLineRequest(Guid ProductId, decimal Quantity);

public sealed record CreatePurchaseOrderRequest(
    Guid SupplierId,
    DateOnly OrderDate,
    string? Notes,
    List<CreatePurchaseOrderLineRequest> Lines);
