using NovaERP.Domain.Common;

namespace NovaERP.Domain.Identity;

/// <summary>
/// Un rol agrupa permisos. Cada tenant tiene su propio conjunto de roles; los
/// marcados IsSystem (Owner, Admin) se siembran al registrar la empresa y no
/// pueden borrarse ni renombrarse desde la UI.
/// </summary>
public sealed class Role : TenantAuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }

    private readonly List<RolePermission> _permissions = [];
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    private Role() { }

    public Role(Guid tenantId, string name, string? description = null, bool isSystem = false)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsSystem = isSystem;
    }

    public void GrantPermission(Guid permissionId)
    {
        if (_permissions.Any(p => p.PermissionId == permissionId))
        {
            return;
        }

        _permissions.Add(new RolePermission(Id, permissionId));
    }
}
