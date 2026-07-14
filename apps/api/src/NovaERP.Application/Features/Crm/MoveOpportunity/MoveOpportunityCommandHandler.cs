using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Crm.Common;

namespace NovaERP.Application.Features.Crm.MoveOpportunity;

public sealed class MoveOpportunityCommandHandler(IApplicationDbContext db)
    : IRequestHandler<MoveOpportunityCommand, OpportunityDto>
{
    public async Task<OpportunityDto> Handle(MoveOpportunityCommand request, CancellationToken ct)
    {
        var opportunity = await db.Opportunities.FirstOrDefaultAsync(o => o.Id == request.OpportunityId, ct)
            ?? throw new ConflictException("La oportunidad no existe.");

        // El agregado rechaza mover una oportunidad ya cerrada (→ 409).
        opportunity.MoveTo(request.Stage, DateOnly.FromDateTime(DateTime.UtcNow));
        await db.SaveChangesAsync(ct);

        var customerName = await db.Partners
            .Where(p => p.Id == opportunity.CustomerId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(ct) ?? "(cliente eliminado)";

        return new OpportunityDto(
            opportunity.Id, opportunity.CustomerId, customerName, opportunity.Title, opportunity.EstimatedValue,
            opportunity.Stage.ToString(), opportunity.ExpectedCloseDate, opportunity.Notes);
    }
}
