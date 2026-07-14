using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Crm;

/// <summary>
/// Etapa de una oportunidad en el pipeline. Won y Lost son terminales: una
/// oportunidad cerrada ya no se mueve.
/// </summary>
public enum OpportunityStage
{
    New = 0,
    Qualified = 1,
    Proposal = 2,
    Won = 3,
    Lost = 4,
}

/// <summary>
/// Oportunidad de venta con un cliente: recorre el pipeline hasta ganarse o
/// perderse. La entidad protege que una oportunidad cerrada no se reabra ni mueva.
/// </summary>
public sealed class Opportunity : TenantAuditableEntity
{
    public Guid CustomerId { get; private set; }
    public string Title { get; private set; } = null!;
    public decimal EstimatedValue { get; private set; }
    public OpportunityStage Stage { get; private set; }
    public DateOnly? ExpectedCloseDate { get; private set; }
    public DateOnly? ClosedOn { get; private set; }
    public string? Notes { get; private set; }

    public bool IsClosed => Stage is OpportunityStage.Won or OpportunityStage.Lost;

    private Opportunity() { }

    public Opportunity(
        Guid tenantId, Guid customerId, string title, decimal estimatedValue,
        DateOnly? expectedCloseDate, string? notes)
    {
        if (estimatedValue < 0)
        {
            throw new DomainException("El valor estimado no puede ser negativo.");
        }

        Id = Guid.NewGuid();
        TenantId = tenantId;
        CustomerId = customerId;
        Title = title.Trim();
        EstimatedValue = estimatedValue;
        ExpectedCloseDate = expectedCloseDate;
        Notes = notes?.Trim();
        Stage = OpportunityStage.New;
    }

    /// <summary>
    /// Mueve la oportunidad a otra etapa. Una oportunidad ya cerrada no se puede
    /// mover. Al pasar a una etapa terminal (Won/Lost) registra la fecha de cierre.
    /// </summary>
    public void MoveTo(OpportunityStage stage, DateOnly asOf)
    {
        if (IsClosed)
        {
            throw new DomainException("La oportunidad ya está cerrada y no se puede mover.");
        }

        Stage = stage;
        ClosedOn = stage is OpportunityStage.Won or OpportunityStage.Lost ? asOf : null;
    }
}
