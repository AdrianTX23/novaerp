using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Accounting.Common;
using NovaERP.Domain.Accounting;

namespace NovaERP.Application.Features.Accounting.GetTrialBalance;

/// <summary>
/// Balance de comprobación: por cada cuenta con movimiento, la suma del debe y
/// del haber, y el saldo según la naturaleza de la cuenta (deudora o acreedora).
/// Si los libros cuadran, el total del debe iguala al del haber.
/// </summary>
public sealed class GetTrialBalanceQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTrialBalanceQuery, TrialBalanceDto>
{
    public async Task<TrialBalanceDto> Handle(GetTrialBalanceQuery request, CancellationToken ct)
    {
        var accounts = await db.Accounts.OrderBy(a => a.Code).ToListAsync(ct);

        // Líneas de los asientos del tenant, agrupadas en memoria (Npgsql no traduce
        // un GroupBy sobre un Contains de subquery).
        var entryIds = await db.JournalEntries.Select(e => e.Id).ToListAsync(ct);
        var lines = await db.JournalEntryLines
            .Where(l => entryIds.Contains(l.JournalEntryId))
            .Select(l => new { l.AccountId, l.Debit, l.Credit })
            .ToListAsync(ct);

        var totalsByAccount = lines
            .GroupBy(l => l.AccountId)
            .ToDictionary(g => g.Key, g => (Debit: g.Sum(x => x.Debit), Credit: g.Sum(x => x.Credit)));

        var rows = accounts
            .Select(a =>
            {
                var (debit, credit) = totalsByAccount.TryGetValue(a.Id, out var t) ? t : (0m, 0m);
                // Deudoras (Activo/Gasto): saldo = debe − haber. Acreedoras: haber − debe.
                var balance = a.Type is AccountType.Asset or AccountType.Expense ? debit - credit : credit - debit;
                return new TrialBalanceRow(a.Id, a.Code, a.Name, a.Type.ToString(), debit, credit, balance);
            })
            .Where(r => r.TotalDebit != 0 || r.TotalCredit != 0)
            .ToList();

        var totalDebit = rows.Sum(r => r.TotalDebit);
        var totalCredit = rows.Sum(r => r.TotalCredit);

        return new TrialBalanceDto(rows, totalDebit, totalCredit, IsBalanced: totalDebit == totalCredit);
    }
}
