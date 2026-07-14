using NovaERP.Domain.Common;

namespace NovaERP.Domain.Accounting;

/// <summary>
/// Naturaleza de una cuenta contable. Determina el signo natural de su saldo:
/// Activo y Gasto son deudoras (saldo = debe − haber); Pasivo, Patrimonio e
/// Ingreso son acreedoras (saldo = haber − debe).
/// </summary>
public enum AccountType
{
    Asset = 0,
    Liability = 1,
    Equity = 2,
    Income = 3,
    Expense = 4,
}

/// <summary>
/// Cuenta del plan contable de una empresa (ej. "1000 Caja"). Tenant-scoped:
/// cada empresa tiene su propio plan, sembrado con uno base al registrarse.
/// </summary>
public sealed class Account : TenantAuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public AccountType Type { get; private set; }
    public bool IsSystem { get; private set; }

    private Account() { }

    public Account(Guid tenantId, string code, string name, AccountType type, bool isSystem = false)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Code = code.Trim();
        Name = name.Trim();
        Type = type;
        IsSystem = isSystem;
    }

    public void Rename(string name) => Name = name.Trim();
}
