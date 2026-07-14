using MediatR;

namespace NovaERP.Application.Features.Cash.DeleteCashMovement;

public sealed record DeleteCashMovementCommand(Guid MovementId) : IRequest;
