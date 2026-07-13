using MediatR;
using NovaERP.Application.Features.Sales.Common;

namespace NovaERP.Application.Features.Sales.ConfirmSalesOrder;

public sealed record ConfirmSalesOrderCommand(Guid OrderId) : IRequest<SalesOrderDetail>;
