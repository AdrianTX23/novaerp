using MediatR;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Cash.Common;
using NovaERP.Domain.Cash;

namespace NovaERP.Application.Features.Cash.CreateCashMovement;

public sealed class CreateCashMovementCommandHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    : IRequestHandler<CreateCashMovementCommand, CashMovementDto>
{
    public async Task<CashMovementDto> Handle(CreateCashMovementCommand request, CancellationToken ct)
    {
        var movement = new CashMovement(
            tenantProvider.TenantId, request.Type, request.Amount, request.Date,
            request.Concept, request.Description);

        db.CashMovements.Add(movement);
        await db.SaveChangesAsync(ct);

        return new CashMovementDto(
            movement.Id, movement.Type.ToString(), movement.Amount, movement.Date,
            movement.Concept, movement.Description, "Manual", true);
    }
}
