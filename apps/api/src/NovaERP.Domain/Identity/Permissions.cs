namespace NovaERP.Domain.Identity;

/// <summary>
/// Catálogo canónico de permisos de NovaERP. Cada constante es el Code que viaja
/// en el JWT y contra el que autoriza el código de negocio. Los Guid son fijos
/// (deterministas) para que el seed sea idempotente entre entornos y migraciones.
/// </summary>
public static class Permissions
{
    public sealed record Definition(Guid Id, string Code, string Description, string Group);

    // Usuarios y accesos
    public const string UsersRead = "users.read";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";

    // Inventario
    public const string InventoryRead = "inventory.read";
    public const string InventoryManage = "inventory.manage";

    // Ventas
    public const string SalesRead = "sales.read";
    public const string SalesManage = "sales.manage";

    // Compras
    public const string PurchasesRead = "purchases.read";
    public const string PurchasesManage = "purchases.manage";

    // Reportes y dashboard
    public const string ReportsRead = "reports.read";

    public static readonly IReadOnlyList<Definition> All =
    [
        new(new Guid("a0000000-0000-0000-0000-000000000001"), UsersRead, "Ver usuarios", "Usuarios"),
        new(new Guid("a0000000-0000-0000-0000-000000000002"), UsersManage, "Crear y editar usuarios", "Usuarios"),
        new(new Guid("a0000000-0000-0000-0000-000000000003"), RolesManage, "Gestionar roles y permisos", "Usuarios"),
        new(new Guid("a0000000-0000-0000-0000-000000000010"), InventoryRead, "Ver inventario", "Inventario"),
        new(new Guid("a0000000-0000-0000-0000-000000000011"), InventoryManage, "Gestionar inventario", "Inventario"),
        new(new Guid("a0000000-0000-0000-0000-000000000020"), SalesRead, "Ver ventas", "Ventas"),
        new(new Guid("a0000000-0000-0000-0000-000000000021"), SalesManage, "Gestionar ventas", "Ventas"),
        new(new Guid("a0000000-0000-0000-0000-000000000030"), PurchasesRead, "Ver compras", "Compras"),
        new(new Guid("a0000000-0000-0000-0000-000000000031"), PurchasesManage, "Gestionar compras", "Compras"),
        new(new Guid("a0000000-0000-0000-0000-000000000040"), ReportsRead, "Ver reportes", "Reportes"),
    ];
}
