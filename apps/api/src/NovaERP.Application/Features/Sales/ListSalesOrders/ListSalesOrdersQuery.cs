using MediatR;
using NovaERP.Application.Features.Sales.Common;

namespace NovaERP.Application.Features.Sales.ListSalesOrders;

public sealed record ListSalesOrdersQuery(string? Status = null, Guid? CustomerId = null)
    : IRequest<List<SalesOrderSummary>>;
