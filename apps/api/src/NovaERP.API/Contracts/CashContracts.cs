using NovaERP.Domain.Cash;

namespace NovaERP.API.Contracts;

public sealed record CreateCashMovementRequest(
    CashMovementType Type, decimal Amount, DateOnly Date, string Concept, string? Description);
