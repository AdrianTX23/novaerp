using NovaERP.Domain.Partners;

namespace NovaERP.Application.Features.Partners.Common;

public sealed record PartnerDto(
    Guid Id,
    string Name,
    PartnerType Type,
    string? DocumentNumber,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);
