using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;

namespace NovaERP.Application.Features.Cash.DeleteCashMovement;

/// <summary>
/// Elimina (soft-delete vía el interceptor) un movimiento manual. Los ingresos
/// por pago de factura no son CashMovements, así que no se pueden borrar aquí.
/// </summary>
public sealed class DeleteCashMovementCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteCashMovementCommand>
{
    public async Task Handle(DeleteCashMovementCommand request, CancellationToken ct)
    {
        var movement = await db.CashMovements.FirstOrDefaultAsync(m => m.Id == request.MovementId, ct)
            ?? throw new ConflictException("El movimiento no existe.");

        db.CashMovements.Remove(movement);
        await db.SaveChangesAsync(ct);
    }
}
