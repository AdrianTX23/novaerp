using NovaERP.Domain.Common;

namespace NovaERP.Domain.Identity;

/// <summary>
/// Catálogo global de permisos. No es tenant-scoped: es el mismo conjunto para
/// todas las empresas (se siembra una vez). El código de negocio autoriza contra
/// el Code (ej. "inventory.read"), nunca contra nombres de rol.
/// </summary>
public sealed class Permission : BaseEntity
{
    public string Code { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    /// <summary>Agrupación para la UI de gestión de roles (ej. "Inventario", "Ventas").</summary>
    public string Group { get; private set; } = null!;

    private Permission() { }

    public Permission(Guid id, string code, string description, string group)
    {
        Id = id;
        Code = code;
        Description = description;
        Group = group;
    }
}
