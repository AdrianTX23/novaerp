using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Crm.Common;
using NovaERP.Application.Features.Crm.CreateOpportunity;
using NovaERP.Application.Features.Crm.GetPipelineSummary;
using NovaERP.Application.Features.Crm.ListOpportunities;
using NovaERP.Application.Features.Crm.MoveOpportunity;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/crm")]
[Authorize]
public sealed class CrmController(IMediator mediator) : ControllerBase
{
    [HttpGet("opportunities")]
    [Authorize(Roles = Permissions.CrmRead)]
    public async Task<ActionResult<List<OpportunityDto>>> Opportunities(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListOpportunitiesQuery(), ct));
    }

    [HttpGet("pipeline")]
    [Authorize(Roles = Permissions.CrmRead)]
    public async Task<ActionResult<PipelineSummaryDto>> Pipeline(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetPipelineSummaryQuery(), ct));
    }

    [HttpPost("opportunities")]
    [Authorize(Roles = Permissions.CrmManage)]
    public async Task<ActionResult<OpportunityDto>> Create(CreateOpportunityRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateOpportunityCommand(
            request.CustomerId, request.Title, request.EstimatedValue, request.ExpectedCloseDate, request.Notes), ct);
        return Ok(result);
    }

    [HttpPost("opportunities/{opportunityId:guid}/move")]
    [Authorize(Roles = Permissions.CrmManage)]
    public async Task<ActionResult<OpportunityDto>> Move(
        Guid opportunityId, MoveOpportunityRequest request, CancellationToken ct)
    {
        return Ok(await mediator.Send(new MoveOpportunityCommand(opportunityId, request.Stage), ct));
    }
}
