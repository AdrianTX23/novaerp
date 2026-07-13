namespace NovaERP.Application.Features.Users.Common;

public sealed record RoleRef(Guid Id, string Name);

public sealed record UserSummary(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    IReadOnlyList<RoleRef> Roles);
