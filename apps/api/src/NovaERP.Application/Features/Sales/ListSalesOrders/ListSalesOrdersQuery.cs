using MediatR;
using NovaERP.Application.Common;
using NovaERP.Application.Features.Sales.Common;

namespace NovaERP.Application.Features.Sales.ListSalesOrders;

public sealed record ListSalesOrdersQuery(string? Status = null, Guid? CustomerId = null, int Page = 1, int PageSize = 50)
    : IRequest<PagedResult<SalesOrderSummary>>;
