using NovaERP.Domain.Crm;

namespace NovaERP.API.Contracts;

public sealed record CreateOpportunityRequest(
    Guid CustomerId, string Title, decimal EstimatedValue, DateOnly? ExpectedCloseDate, string? Notes);

public sealed record MoveOpportunityRequest(OpportunityStage Stage);
