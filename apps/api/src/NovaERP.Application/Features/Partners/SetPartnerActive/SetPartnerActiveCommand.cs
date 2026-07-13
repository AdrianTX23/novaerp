using MediatR;
using NovaERP.Application.Features.Partners.Common;

namespace NovaERP.Application.Features.Partners.SetPartnerActive;

public sealed record SetPartnerActiveCommand(Guid PartnerId, bool IsActive) : IRequest<PartnerDto>;
