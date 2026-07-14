using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Accounting;

/// <summary>
/// Asiento contable (partida doble). Raíz del agregado que agrupa sus líneas y
/// protege la invariante sagrada de la contabilidad: la suma del debe debe ser
/// igual a la suma del haber, y mayor que cero.
/// </summary>
public sealed class JournalEntry : TenantAuditableEntity
{
    public string Number { get; private set; } = null!;
    public DateOnly Date { get; private set; }
    public string Description { get; private set; } = null!;
    public string? Reference { get; private set; }
    public decimal Total { get; private set; }

    private readonly List<JournalEntryLine> _lines = [];
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    private JournalEntry() { }

    public JournalEntry(Guid tenantId, string number, DateOnly date, string description, string? reference)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Number = number;
        Date = date;
        Description = description.Trim();
        Reference = reference?.Trim();
    }

    public void AddLine(JournalEntryLine line) => _lines.Add(line);

    /// <summary>
    /// Valida que el asiento cuadre. Debe llamarse antes de persistir; lanza
    /// DomainException si el debe y el haber no coinciden o el total es cero.
    /// </summary>
    public void EnsureBalanced()
    {
        if (_lines.Count < 2)
        {
            throw new DomainException("Un asiento necesita al menos dos líneas.");
        }

        var totalDebit = _lines.Sum(l => l.Debit);
        var totalCredit = _lines.Sum(l => l.Credit);

        if (totalDebit == 0)
        {
            throw new DomainException("El asiento no puede tener importe cero.");
        }

        if (totalDebit != totalCredit)
        {
            throw new DomainException($"El asiento no cuadra: debe {totalDebit:0.00} ≠ haber {totalCredit:0.00}.");
        }

        Total = totalDebit;
    }
}
