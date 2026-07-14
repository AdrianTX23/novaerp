using MediatR;
using NovaERP.Application.Features.Cash.Common;
using NovaERP.Domain.Cash;

namespace NovaERP.Application.Features.Cash.CreateCashMovement;

public sealed record CreateCashMovementCommand(
    CashMovementType Type,
    decimal Amount,
    DateOnly Date,
    string Concept,
    string? Description) : IRequest<CashMovementDto>;
