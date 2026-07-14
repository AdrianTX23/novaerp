using MediatR;
using NovaERP.Application.Features.Accounting.Common;

namespace NovaERP.Application.Features.Accounting.CreateJournalEntry;

public sealed record JournalLineInput(Guid AccountId, decimal Debit, decimal Credit);

public sealed record CreateJournalEntryCommand(
    DateOnly Date,
    string Description,
    string? Reference,
    List<JournalLineInput> Lines) : IRequest<JournalEntryDetail>;
