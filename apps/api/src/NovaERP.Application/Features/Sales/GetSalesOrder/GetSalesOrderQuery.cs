using MediatR;
using NovaERP.Application.Features.Sales.Common;

namespace NovaERP.Application.Features.Sales.GetSalesOrder;

public sealed record GetSalesOrderQuery(Guid OrderId) : IRequest<SalesOrderDetail>;
