using MediatR;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.CreateProduct;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string UnitOfMeasure,
    decimal CostPrice,
    decimal SalePrice,
    Guid? CategoryId,
    string? Description,
    decimal? ReorderPoint,
    decimal InitialQuantity) : IRequest<ProductSummary>;
