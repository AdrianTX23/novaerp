namespace NovaERP.API.Contracts;

public sealed record CreateSalesOrderLineRequest(Guid ProductId, decimal Quantity);

public sealed record CreateSalesOrderRequest(
    Guid CustomerId,
    DateOnly OrderDate,
    string? Notes,
    List<CreateSalesOrderLineRequest> Lines);
