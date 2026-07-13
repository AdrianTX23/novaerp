using NovaERP.Domain.Partners;

namespace NovaERP.API.Contracts;

public sealed record CreatePartnerRequest(
    string Name, PartnerType Type, string? DocumentNumber, string? Email, string? Phone, string? Address);

public sealed record UpdatePartnerRequest(
    string Name, PartnerType Type, string? DocumentNumber, string? Email, string? Phone, string? Address);
