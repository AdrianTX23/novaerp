using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Invoicing.Common;

namespace NovaERP.Application.Features.Invoicing.RegisterPayment;

public sealed class RegisterPaymentCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RegisterPaymentCommand, InvoiceDetail>
{
    public async Task<InvoiceDetail> Handle(RegisterPaymentCommand request, CancellationToken ct)
    {
        var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct)
            ?? throw new ConflictException("La factura no existe.");

        // El agregado valida saldo, estado y monto; lanza DomainException (→ 409).
        var payment = invoice.RegisterPayment(request.Amount, request.PaidAt, request.Method, request.Reference?.Trim());

        // Se agrega el pago vía su DbSet para que EF lo inserte (ver Invoice.RegisterPayment).
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);

        return InvoiceMapper.ToDetail(invoice);
    }
}
