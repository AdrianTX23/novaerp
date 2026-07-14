using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Invoicing.Common;
using NovaERP.Domain.Invoicing;
using NovaERP.Domain.Sales;

namespace NovaERP.Application.Features.Invoicing.CreateInvoice;

public sealed class CreateInvoiceCommandHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    : IRequestHandler<CreateInvoiceCommand, InvoiceDetail>
{
    private const int DefaultPaymentTermDays = 30;

    public async Task<InvoiceDetail> Handle(CreateInvoiceCommand request, CancellationToken ct)
    {
        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == request.SalesOrderId, ct)
            ?? throw new ConflictException("El pedido de venta no existe.");

        if (order.Status != SalesOrderStatus.Confirmed)
        {
            throw new ConflictException("Solo se puede facturar un pedido confirmado.");
        }

        if (await db.Invoices.AnyAsync(i => i.SalesOrderId == order.Id, ct))
        {
            throw new ConflictException("Este pedido ya tiene una factura emitida.");
        }

        var customerName = await db.Partners
            .Where(p => p.Id == order.CustomerId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(ct) ?? "(cliente eliminado)";

        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueDate = request.DueDate ?? issueDate.AddDays(DefaultPaymentTermDays);

        var invoice = new Invoice(
            tenantProvider.TenantId,
            await GenerateInvoiceNumberAsync(ct),
            order.Id,
            order.CustomerId,
            customerName,
            issueDate,
            dueDate,
            request.Notes?.Trim());

        // Snapshot de las líneas del pedido al momento de emitir.
        foreach (var line in order.Lines)
        {
            invoice.AddLine(new InvoiceLine(line.ProductSku, line.ProductName, line.Quantity, line.UnitPrice));
        }

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(ct);

        return InvoiceMapper.ToDetail(invoice);
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken ct)
    {
        var count = await db.Invoices.CountAsync(ct);
        return $"INV-{count + 1:D5}";
    }
}
