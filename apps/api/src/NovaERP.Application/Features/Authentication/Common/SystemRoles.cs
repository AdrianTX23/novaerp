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
    public const string Member = "Member";

    public static Role CreateOwner(Guid tenantId)
    {
        var role = new Role(tenantId, Owner, "Acceso total a la empresa", isSystem: true);
        foreach (var permission in Permissions.All)
        {
            role.GrantPermission(permission.Id);
        }
        return role;
    }

    /// <summary>Todo salvo gestionar roles: pensado para delegar operación sin ceder control de accesos.</summary>
    public static Role CreateAdmin(Guid tenantId)
    {
        var role = new Role(tenantId, Admin, "Gestiona la operación diaria, sin administrar roles", isSystem: true);
        foreach (var permission in Permissions.All.Where(p => p.Code != Permissions.RolesManage))
        {
            role.GrantPermission(permission.Id);
        }
        return role;
    }

    /// <summary>Solo lectura en todos los módulos.</summary>
    public static Role CreateMember(Guid tenantId)
    {
        var role = new Role(tenantId, Member, "Solo lectura en todos los módulos", isSystem: true);
        foreach (var permission in Permissions.All.Where(p => p.Code.EndsWith(".read")))
        {
            role.GrantPermission(permission.Id);
        }
        return role;
    }
}
