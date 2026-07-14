using MediatR;
using NovaERP.Application.Features.Cash.Common;

namespace NovaERP.Application.Features.Cash.ListCashMovements;

public sealed record ListCashMovementsQuery(DateOnly? From = null, DateOnly? To = null)
    : IRequest<List<CashMovementDto>>;
