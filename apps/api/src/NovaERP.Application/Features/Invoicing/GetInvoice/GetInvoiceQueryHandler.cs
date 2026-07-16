using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Invoicing.Common;

namespace NovaERP.Application.Features.Invoicing.GetInvoice;

public sealed class GetInvoiceQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInvoiceQuery, InvoiceDetail>
{
    public async Task<InvoiceDetail> Handle(GetInvoiceQuery request, CancellationToken ct)
    {
        var invoice = await db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct)
            ?? throw new ConflictException("La factura no existe.");

        return InvoiceMapper.ToDetail(invoice);
    }
}
