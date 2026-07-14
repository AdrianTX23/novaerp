using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Accounting.Common;

namespace NovaERP.Application.Features.Accounting.ListJournalEntries;

public sealed class ListJournalEntriesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListJournalEntriesQuery, List<JournalEntrySummary>>
{
    public async Task<List<JournalEntrySummary>> Handle(ListJournalEntriesQuery request, CancellationToken ct)
    {
        return await db.JournalEntries
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Number)
            .Select(e => new JournalEntrySummary(
                e.Id, e.Number, e.Date, e.Description, e.Reference, e.Total))
            .ToListAsync(ct);
    }
}
