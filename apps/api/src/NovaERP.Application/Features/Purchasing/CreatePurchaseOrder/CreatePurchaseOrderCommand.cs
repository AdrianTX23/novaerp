using MediatR;
using NovaERP.Application.Features.Purchasing.Common;

namespace NovaERP.Application.Features.Purchasing.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderLineInput(Guid ProductId, decimal Quantity);

public sealed record CreatePurchaseOrderCommand(
    Guid SupplierId,
    DateOnly OrderDate,
    string? Notes,
    List<CreatePurchaseOrderLineInput> Lines) : IRequest<PurchaseOrderDetail>;
