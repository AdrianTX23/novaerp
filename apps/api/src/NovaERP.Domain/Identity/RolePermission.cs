using NovaERP.Domain.Common;

namespace NovaERP.Domain.Identity;

/// <summary>Join Role ↔ Permission. Se accede vía la navegación de Role (tenant-scoped).</summary>
public sealed class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        Id = Guid.NewGuid();
        RoleId = roleId;
        PermissionId = permissionId;
    }
}
