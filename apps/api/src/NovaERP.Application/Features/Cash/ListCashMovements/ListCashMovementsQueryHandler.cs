using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Cash.Common;

namespace NovaERP.Application.Features.Cash.ListCashMovements;

/// <summary>
/// Lista unificada de movimientos: los pagos de facturas (ingresos, de solo
/// lectura) y los movimientos manuales (eliminables). Se materializan ambas
/// fuentes y se combinan/ordenan por fecha en memoria.
/// </summary>
public sealed class ListCashMovementsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListCashMovementsQuery, List<CashMovementDto>>
{
    public async Task<List<CashMovementDto>> Handle(ListCashMovementsQuery request, CancellationToken ct)
    {
        var manual = await db.CashMovements
            .Where(m => (request.From == null || m.Date >= request.From)
                     && (request.To == null || m.Date <= request.To))
            .Select(m => new CashMovementDto(
                m.Id, m.Type.ToString(), m.Amount, m.Date, m.Concept, m.Description, "Manual", true))
            .ToListAsync(ct);

        var payments = await db.Payments
            .Where(p => (request.From == null || p.PaidAt >= request.From)
                     && (request.To == null || p.PaidAt <= request.To))
            .Select(p => new CashMovementDto(
                p.Id,
                "Income",
                p.Amount,
                p.PaidAt,
                "Cobro " + db.Invoices.Where(i => i.Id == p.InvoiceId).Select(i => i.InvoiceNumber).FirstOrDefault(),
                p.Reference,
                "Invoice",
                false))
            .ToListAsync(ct);

        return manual
            .Concat(payments)
            .OrderByDescending(m => m.Date)
            .ToList();
    }
}
