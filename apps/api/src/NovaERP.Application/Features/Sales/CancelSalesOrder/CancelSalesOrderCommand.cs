using MediatR;
using NovaERP.Application.Features.Sales.Common;

namespace NovaERP.Application.Features.Sales.CancelSalesOrder;

public sealed record CancelSalesOrderCommand(Guid OrderId) : IRequest<SalesOrderDetail>;
