using MediatR;
using NovaERP.Application.Features.Partners.Common;
using NovaERP.Domain.Partners;

namespace NovaERP.Application.Features.Partners.CreatePartner;

public sealed record CreatePartnerCommand(
    string Name,
    PartnerType Type,
    string? DocumentNumber,
    string? Email,
    string? Phone,
    string? Address) : IRequest<PartnerDto>;
