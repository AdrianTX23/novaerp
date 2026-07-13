namespace NovaERP.Application.Features.Catalog.Common;

public sealed record CategoryDto(Guid Id, string Name, string? Description, int ProductCount);

public sealed record ProductSummary(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    string UnitOfMeasure,
    decimal CostPrice,
    decimal SalePrice,
    decimal QuantityOnHand,
    decimal? ReorderPoint,
    bool IsActive,
    bool IsLowStock);
