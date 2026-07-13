using MediatR;
using NovaERP.Application.Features.Partners.Common;
using NovaERP.Domain.Partners;

namespace NovaERP.Application.Features.Partners.ListPartners;

public sealed record ListPartnersQuery(PartnerType? Type = null) : IRequest<List<PartnerDto>>;
