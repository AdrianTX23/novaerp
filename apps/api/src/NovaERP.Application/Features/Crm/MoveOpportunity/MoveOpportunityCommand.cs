using MediatR;
using NovaERP.Application.Features.Crm.Common;
using NovaERP.Domain.Crm;

namespace NovaERP.Application.Features.Crm.MoveOpportunity;

public sealed record MoveOpportunityCommand(Guid OpportunityId, OpportunityStage Stage) : IRequest<OpportunityDto>;
