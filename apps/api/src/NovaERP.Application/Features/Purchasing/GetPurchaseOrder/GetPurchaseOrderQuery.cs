using MediatR;
using NovaERP.Application.Features.Purchasing.Common;

namespace NovaERP.Application.Features.Purchasing.GetPurchaseOrder;

public sealed record GetPurchaseOrderQuery(Guid OrderId) : IRequest<PurchaseOrderDetail>;
