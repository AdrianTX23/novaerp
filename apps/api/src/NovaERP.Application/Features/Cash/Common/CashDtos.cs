namespace NovaERP.Application.Features.Cash.Common;

/// <summary>
/// Un movimiento de caja unificado: puede provenir de un pago de factura
/// (Source = "Invoice", solo lectura) o de un registro manual (Source = "Manual",
/// eliminable). Kind es "Income" o "Expense".
/// </summary>
public sealed record CashMovementDto(
    Guid Id,
    string Kind,
    decimal Amount,
    DateOnly Date,
    string Concept,
    string? Description,
    string Source,
    bool CanDelete);

public sealed record CashSummaryDto(
    decimal Balance,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal IncomeThisMonth,
    decimal ExpenseThisMonth);
