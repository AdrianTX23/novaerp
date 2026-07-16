using MediatR;
using NovaERP.Application.Common;
using NovaERP.Application.Features.Partners.Common;
using NovaERP.Domain.Partners;

namespace NovaERP.Application.Features.Partners.ListPartners;

public sealed record ListPartnersQuery(PartnerType? Type = null, int Page = 1, int PageSize = 50)
    : IRequest<PagedResult<PartnerDto>>;
