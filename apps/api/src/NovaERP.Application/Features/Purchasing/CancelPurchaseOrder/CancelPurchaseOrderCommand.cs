using MediatR;
using NovaERP.Application.Features.Purchasing.Common;

namespace NovaERP.Application.Features.Purchasing.CancelPurchaseOrder;

public sealed record CancelPurchaseOrderCommand(Guid OrderId) : IRequest<PurchaseOrderDetail>;
