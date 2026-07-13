using MediatR;
using NovaERP.Application.Features.Purchasing.Common;

namespace NovaERP.Application.Features.Purchasing.ListPurchaseOrders;

public sealed record ListPurchaseOrdersQuery(string? Status = null, Guid? SupplierId = null)
    : IRequest<List<PurchaseOrderSummary>>;
