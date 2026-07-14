using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Invoicing.Common;

namespace NovaERP.Application.Features.Invoicing.VoidInvoice;

public sealed class VoidInvoiceCommandHandler(IApplicationDbContext db)
    : IRequestHandler<VoidInvoiceCommand, InvoiceDetail>
{
    public async Task<InvoiceDetail> Handle(VoidInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct)
            ?? throw new ConflictException("La factura no existe.");

        // El agregado rechaza anular una factura con pagos registrados.
        invoice.Void();
        await db.SaveChangesAsync(ct);

        return InvoiceMapper.ToDetail(invoice);
    }
}
