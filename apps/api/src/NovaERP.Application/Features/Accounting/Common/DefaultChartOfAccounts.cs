using NovaERP.Domain.Accounting;

namespace NovaERP.Application.Features.Accounting.Common;

/// <summary>
/// Plan de cuentas base que se siembra al registrar una empresa, para que "abra
/// los libros" lista. El Owner puede agregar más cuentas después.
/// </summary>
public static class DefaultChartOfAccounts
{
    private static readonly (string Code, string Name, AccountType Type)[] Accounts =
    [
        ("1000", "Caja", AccountType.Asset),
        ("1010", "Bancos", AccountType.Asset),
        ("1100", "Clientes por cobrar", AccountType.Asset),
        ("1200", "Inventario", AccountType.Asset),
        ("2000", "Proveedores por pagar", AccountType.Liability),
        ("2100", "Impuestos por pagar", AccountType.Liability),
        ("3000", "Capital", AccountType.Equity),
        ("4000", "Ventas", AccountType.Income),
        ("5000", "Costo de ventas", AccountType.Expense),
        ("6000", "Gastos operativos", AccountType.Expense),
    ];

    public static IEnumerable<Account> CreateFor(Guid tenantId) =>
        Accounts.Select(a => new Account(tenantId, a.Code, a.Name, a.Type, isSystem: true));
}
