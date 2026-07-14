using MediatR;
using NovaERP.Application.Features.Accounting.Common;

namespace NovaERP.Application.Features.Accounting.ListJournalEntries;

public sealed record ListJournalEntriesQuery : IRequest<List<JournalEntrySummary>>;
