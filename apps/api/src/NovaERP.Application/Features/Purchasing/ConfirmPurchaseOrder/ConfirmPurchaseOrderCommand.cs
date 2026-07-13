using MediatR;
using NovaERP.Application.Features.Purchasing.Common;

namespace NovaERP.Application.Features.Purchasing.ConfirmPurchaseOrder;

public sealed record ConfirmPurchaseOrderCommand(Guid OrderId) : IRequest<PurchaseOrderDetail>;
