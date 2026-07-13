using MediatR;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string UnitOfMeasure,
    decimal CostPrice,
    decimal SalePrice,
    Guid? CategoryId,
    string? Description,
    decimal? ReorderPoint) : IRequest<ProductSummary>;
