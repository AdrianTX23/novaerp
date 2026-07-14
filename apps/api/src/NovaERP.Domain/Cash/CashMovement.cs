using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Cash;

public enum CashMovementType
{
    Income = 0,
    Expense = 1,
}

/// <summary>
/// Movimiento de caja registrado a mano (renta, sueldos, un ingreso suelto…).
/// Los ingresos por cobro de facturas NO son CashMovements: viven en Payment y
/// la Caja los unifica al leer. Esta entidad es solo para ajustes manuales.
/// </summary>
public sealed class CashMovement : TenantAuditableEntity
{
    public CashMovementType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public string Concept { get; private set; } = null!;
    public string? Description { get; private set; }

    private CashMovement() { }

    public CashMovement(Guid tenantId, CashMovementType type, decimal amount, DateOnly date, string concept, string? description)
    {
        if (amount <= 0)
        {
            throw new DomainException("El monto del movimiento debe ser mayor que cero.");
        }

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Type = type;
        Amount = amount;
        Date = date;
        Concept = concept.Trim();
        Description = description?.Trim();
    }
}
