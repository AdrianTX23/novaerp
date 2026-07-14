using MediatR;
using NovaERP.Application.Features.Crm.Common;

namespace NovaERP.Application.Features.Crm.CreateOpportunity;

public sealed record CreateOpportunityCommand(
    Guid CustomerId,
    string Title,
    decimal EstimatedValue,
    DateOnly? ExpectedCloseDate,
    string? Notes) : IRequest<OpportunityDto>;
