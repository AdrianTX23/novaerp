using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Purchasing.Common;

namespace NovaERP.Application.Features.Purchasing.GetPurchaseOrder;

public sealed class GetPurchaseOrderQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPurchaseOrderQuery, PurchaseOrderDetail>
{
    public async Task<PurchaseOrderDetail> Handle(GetPurchaseOrderQuery request, CancellationToken ct)
    {
        var order = await db.PurchaseOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new ConflictException("La orden no existe.");

        return await PurchaseOrderDetailFactory.CreateAsync(db, order, ct);
    }
}
