using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Crm.Common;
using NovaERP.Domain.Crm;
using NovaERP.Domain.Partners;

namespace NovaERP.Application.Features.Crm.CreateOpportunity;

public sealed class CreateOpportunityCommandHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    : IRequestHandler<CreateOpportunityCommand, OpportunityDto>
{
    public async Task<OpportunityDto> Handle(CreateOpportunityCommand request, CancellationToken ct)
    {
        var customer = await db.Partners.FirstOrDefaultAsync(p => p.Id == request.CustomerId, ct)
            ?? throw new ConflictException("El cliente no existe.");

        if ((customer.Type & PartnerType.Customer) != PartnerType.Customer)
        {
            throw new ConflictException("El contacto seleccionado no es un cliente.");
        }

        var opportunity = new Opportunity(
            tenantProvider.TenantId, customer.Id, request.Title, request.EstimatedValue,
            request.ExpectedCloseDate, request.Notes);

        db.Opportunities.Add(opportunity);
        await db.SaveChangesAsync(ct);

        return new OpportunityDto(
            opportunity.Id, customer.Id, customer.Name, opportunity.Title, opportunity.EstimatedValue,
            opportunity.Stage.ToString(), opportunity.ExpectedCloseDate, opportunity.Notes);
    }
}
