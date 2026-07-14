using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Reports.Common;
using NovaERP.Domain.Invoicing;

namespace NovaERP.Application.Features.Reports.GetReceivablesReport;

/// <summary>
/// Cuentas por cobrar: facturas emitidas o parcialmente pagadas (Paid y Void
/// quedan fuera), con antigüedad de saldo respecto a hoy. "Al día" incluye lo
/// que aún no vence; luego cubetas de 1-30 / 31-60 / 60+ días de mora.
/// </summary>
public sealed class GetReceivablesReportQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetReceivablesReportQuery, ReceivablesReportDto>
{
    public async Task<ReceivablesReportDto> Handle(GetReceivablesReportQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var pending = await db.Invoices
            .Where(i => i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid)
            .Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.CustomerName,
                i.DueDate,
                Outstanding = i.Total - i.AmountPaid,
            })
            .ToListAsync(ct);

        var rows = pending
            .Select(i =>
            {
                var daysOverdue = today.DayNumber - i.DueDate.DayNumber;
                var bucket = BucketFor(daysOverdue);
                return new ReceivableRow(i.Id, i.InvoiceNumber, i.CustomerName, i.DueDate, i.Outstanding, Math.Max(0, daysOverdue), bucket);
            })
            .OrderByDescending(r => r.DaysOverdue)
            .ToList();

        var buckets = new[] { "Al día", "1-30 días", "31-60 días", "60+ días" }
            .Select(name => new AgingBucketTotal(
                name,
                rows.Where(r => r.Bucket == name).Sum(r => r.OutstandingBalance),
                rows.Count(r => r.Bucket == name)))
            .ToList();

        return new ReceivablesReportDto(rows.Sum(r => r.OutstandingBalance), buckets, rows);
    }

    private static string BucketFor(int daysOverdue) => daysOverdue switch
    {
        <= 0 => "Al día",
        <= 30 => "1-30 días",
        <= 60 => "31-60 días",
        _ => "60+ días",
    };
}
