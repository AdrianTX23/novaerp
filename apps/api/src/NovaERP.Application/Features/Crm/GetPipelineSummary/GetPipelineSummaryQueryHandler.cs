using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Crm.Common;
using NovaERP.Domain.Crm;

namespace NovaERP.Application.Features.Crm.GetPipelineSummary;

public sealed class GetPipelineSummaryQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPipelineSummaryQuery, PipelineSummaryDto>
{
    private static readonly OpportunityStage[] AllStages =
        [OpportunityStage.New, OpportunityStage.Qualified, OpportunityStage.Proposal, OpportunityStage.Won, OpportunityStage.Lost];

    public async Task<PipelineSummaryDto> Handle(GetPipelineSummaryQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        // Volumen acotado por tenant: se materializa y se agrupa en memoria.
        var opps = await db.Opportunities
            .Select(o => new { o.Stage, o.EstimatedValue, o.ClosedOn })
            .ToListAsync(ct);

        var open = opps.Where(o => o.Stage is not (OpportunityStage.Won or OpportunityStage.Lost)).ToList();

        var byStage = AllStages
            .Select(stage =>
            {
                var inStage = opps.Where(o => o.Stage == stage).ToList();
                return new StageSummary(stage.ToString(), inStage.Count, inStage.Sum(o => o.EstimatedValue));
            })
            .ToList();

        var wonThisMonth = opps
            .Where(o => o.Stage == OpportunityStage.Won && o.ClosedOn >= monthStart && o.ClosedOn < nextMonth)
            .Sum(o => o.EstimatedValue);

        return new PipelineSummaryDto(
            OpenValue: open.Sum(o => o.EstimatedValue),
            OpenCount: open.Count,
            WonThisMonth: wonThisMonth,
            ByStage: byStage);
    }
}
