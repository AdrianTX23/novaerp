using MediatR;
using NovaERP.Application.Features.Sales.Common;

namespace NovaERP.Application.Features.Sales.CreateSalesOrder;

public sealed record CreateSalesOrderLineInput(Guid ProductId, decimal Quantity);

public sealed record CreateSalesOrderCommand(
    Guid CustomerId,
    DateOnly OrderDate,
    string? Notes,
    List<CreateSalesOrderLineInput> Lines) : IRequest<SalesOrderDetail>;
