namespace NovaERP.Application.Features.Roles.Common;

public sealed record RoleDetail(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    int UserCount,
    IReadOnlyList<string> PermissionCodes);

public sealed record PermissionDto(Guid Id, string Code, string Description, string Group);
