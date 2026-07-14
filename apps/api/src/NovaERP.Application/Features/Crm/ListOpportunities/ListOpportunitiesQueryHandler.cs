using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Crm.Common;

namespace NovaERP.Application.Features.Crm.ListOpportunities;

public sealed class ListOpportunitiesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListOpportunitiesQuery, List<OpportunityDto>>
{
    public async Task<List<OpportunityDto>> Handle(ListOpportunitiesQuery request, CancellationToken ct)
    {
        return await db.Opportunities
            .OrderByDescending(o => o.EstimatedValue)
            .Select(o => new OpportunityDto(
                o.Id,
                o.CustomerId,
                db.Partners.Where(p => p.Id == o.CustomerId).Select(p => p.Name).FirstOrDefault() ?? "(cliente eliminado)",
                o.Title,
                o.EstimatedValue,
                o.Stage.ToString(),
                o.ExpectedCloseDate,
                o.Notes))
            .ToListAsync(ct);
    }
}
