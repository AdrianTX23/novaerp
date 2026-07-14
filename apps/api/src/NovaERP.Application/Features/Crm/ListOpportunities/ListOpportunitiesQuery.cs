using MediatR;
using NovaERP.Application.Features.Crm.Common;

namespace NovaERP.Application.Features.Crm.ListOpportunities;

public sealed record ListOpportunitiesQuery : IRequest<List<OpportunityDto>>;
