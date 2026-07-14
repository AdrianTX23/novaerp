using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Accounting.Common;
using NovaERP.Domain.Accounting;

namespace NovaERP.Application.Features.Accounting.CreateJournalEntry;

public sealed class CreateJournalEntryCommandHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    : IRequestHandler<CreateJournalEntryCommand, JournalEntryDetail>
{
    public async Task<JournalEntryDetail> Handle(CreateJournalEntryCommand request, CancellationToken ct)
    {
        var accountIds = request.Lines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = await db.Accounts.Where(a => accountIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, ct);
        if (accounts.Count != accountIds.Count)
        {
            throw new ConflictException("Una o más cuentas no existen.");
        }

        var entry = new JournalEntry(
            tenantProvider.TenantId, await GenerateNumberAsync(ct), request.Date,
            request.Description, request.Reference);

        foreach (var line in request.Lines)
        {
            // El constructor de la línea valida debe/haber; el asiento valida el equilibrio.
            entry.AddLine(new JournalEntryLine(line.AccountId, line.Debit, line.Credit));
        }

        // Invariante de partida doble: Σdebe = Σhaber. Lanza DomainException (→ 409).
        entry.EnsureBalanced();

        db.JournalEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        var lines = entry.Lines
            .Select(l => new JournalEntryLineDto(
                l.AccountId, accounts[l.AccountId].Code, accounts[l.AccountId].Name, l.Debit, l.Credit))
            .ToList();

        return new JournalEntryDetail(
            entry.Id, entry.Number, entry.Date, entry.Description, entry.Reference, entry.Total, lines);
    }

    private async Task<string> GenerateNumberAsync(CancellationToken ct)
    {
        var count = await db.JournalEntries.CountAsync(ct);
        return $"ASI-{count + 1:D5}";
    }
}
