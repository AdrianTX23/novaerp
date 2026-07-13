using NovaERP.Domain.Identity;

namespace NovaERP.Application.Features.Authentication.Common;

/// <summary>
/// Roles sembrados automáticamente al registrar un tenant. Owner recibe todos
/// los permisos; el resto son plantillas que el Owner puede ajustar después.
/// </summary>
public static class SystemRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";

    public static Role CreateOwner(Guid tenantId)
    {
        var role = new Role(tenantId, Owner, "Acceso total a la empresa", isSystem: true);
        foreach (var permission in Permissions.All)
        {
            role.GrantPermission(permission.Id);
        }
        return role;
    }
}
