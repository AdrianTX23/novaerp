using MediatR;
using NovaERP.Application.Common;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.ListProducts;

public sealed record ListProductsQuery(
    string? Search = null, Guid? CategoryId = null, bool LowStockOnly = false, int Page = 1, int PageSize = 50)
    : IRequest<PagedResult<ProductSummary>>;
