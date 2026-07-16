namespace NovaERP.Application.Features.Settings.Common;

public sealed record CompanyDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    DateTimeOffset CreatedAt);
