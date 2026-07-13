using MediatR;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Partners.Common;
using NovaERP.Domain.Partners;

namespace NovaERP.Application.Features.Partners.CreatePartner;

public sealed class CreatePartnerCommandHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    : IRequestHandler<CreatePartnerCommand, PartnerDto>
{
    public async Task<PartnerDto> Handle(CreatePartnerCommand request, CancellationToken ct)
    {
        var partner = new Partner(
            tenantProvider.TenantId, request.Name, request.Type,
            request.DocumentNumber?.Trim(), request.Email?.Trim(), request.Phone?.Trim(), request.Address?.Trim());

        db.Partners.Add(partner);
        await db.SaveChangesAsync(ct);

        return partner.ToDto();
    }
}
