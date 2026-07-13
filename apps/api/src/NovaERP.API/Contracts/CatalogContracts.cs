namespace NovaERP.API.Contracts;

public sealed record CreateCategoryRequest(string Name, string? Description);

public sealed record UpdateCategoryRequest(string Name, string? Description);

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string UnitOfMeasure,
    decimal CostPrice,
    decimal SalePrice,
    Guid? CategoryId,
    string? Description,
    decimal? ReorderPoint,
    decimal InitialQuantity);

public sealed record UpdateProductRequest(
    string Name,
    string UnitOfMeasure,
    decimal CostPrice,
    decimal SalePrice,
    Guid? CategoryId,
    string? Description,
    decimal? ReorderPoint);

public sealed record AdjustStockRequest(decimal Delta);
