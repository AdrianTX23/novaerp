namespace NovaERP.Application.Features.Accounting.Common;

public sealed record AccountDto(Guid Id, string Code, string Name, string Type, bool IsSystem);

public sealed record JournalEntryLineDto(
    Guid AccountId, string AccountCode, string AccountName, decimal Debit, decimal Credit);

public sealed record JournalEntrySummary(
    Guid Id, string Number, DateOnly Date, string Description, string? Reference, decimal Total);

public sealed record JournalEntryDetail(
    Guid Id, string Number, DateOnly Date, string Description, string? Reference, decimal Total,
    IReadOnlyList<JournalEntryLineDto> Lines);

/// <summary>Una fila del balance de comprobación: totales por cuenta y su saldo.</summary>
public sealed record TrialBalanceRow(
    Guid AccountId, string Code, string Name, string Type, decimal TotalDebit, decimal TotalCredit, decimal Balance);

public sealed record TrialBalanceDto(
    IReadOnlyList<TrialBalanceRow> Rows, decimal TotalDebit, decimal TotalCredit, bool IsBalanced);
