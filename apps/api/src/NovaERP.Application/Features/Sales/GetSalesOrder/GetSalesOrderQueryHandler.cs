using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Sales.Common;

namespace NovaERP.Application.Features.Sales.GetSalesOrder;

public sealed class GetSalesOrderQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSalesOrderQuery, SalesOrderDetail>
{
    public async Task<SalesOrderDetail> Handle(GetSalesOrderQuery request, CancellationToken ct)
    {
        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new ConflictException("El pedido no existe.");

        return await SalesOrderDetailFactory.CreateAsync(db, order, ct);
    }
}
