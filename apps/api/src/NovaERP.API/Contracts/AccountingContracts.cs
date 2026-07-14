using NovaERP.Domain.Accounting;

namespace NovaERP.API.Contracts;

public sealed record CreateAccountRequest(string Code, string Name, AccountType Type);

public sealed record JournalLineRequest(Guid AccountId, decimal Debit, decimal Credit);

public sealed record CreateJournalEntryRequest(
    DateOnly Date, string Description, string? Reference, List<JournalLineRequest> Lines);
