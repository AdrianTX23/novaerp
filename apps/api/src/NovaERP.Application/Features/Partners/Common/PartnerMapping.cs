using NovaERP.Domain.Partners;

namespace NovaERP.Application.Features.Partners.Common;

public static class PartnerMapping
{
    public static PartnerDto ToDto(this Partner partner) => new(
        partner.Id, partner.Name, partner.Type, partner.DocumentNumber,
        partner.Email, partner.Phone, partner.Address, partner.IsActive);
}
