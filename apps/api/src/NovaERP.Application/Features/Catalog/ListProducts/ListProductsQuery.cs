using MediatR;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.ListProducts;

public sealed record ListProductsQuery(string? Search = null, Guid? CategoryId = null, bool LowStockOnly = false)
    : IRequest<List<ProductSummary>>;
