namespace NovaERP.API.Contracts;

public sealed record CreateUserRequest(string Email, string FullName, string Password, List<Guid> RoleIds);

public sealed record UpdateUserRolesRequest(List<Guid> RoleIds);

public sealed record CreateRoleRequest(string Name, string? Description, List<string> PermissionCodes);

public sealed record UpdateRoleRequest(string Name, string? Description, List<string> PermissionCodes);
