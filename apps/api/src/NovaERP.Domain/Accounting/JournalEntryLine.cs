using NovaERP.Domain.Common;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.Domain.Accounting;

/// <summary>
/// Línea de un asiento: carga a una cuenta al debe O al haber (nunca ambos, ni
/// ninguno). El equilibrio del asiento (Σdebe = Σhaber) lo valida JournalEntry.
/// </summary>
public sealed class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    private JournalEntryLine() { }

    public JournalEntryLine(Guid accountId, decimal debit, decimal credit)
    {
        if (debit < 0 || credit < 0)
        {
            throw new DomainException("El debe y el haber no pueden ser negativos.");
        }

        if ((debit > 0) == (credit > 0))
        {
            throw new DomainException("Cada línea debe cargar al debe o al haber, pero no a ambos.");
        }

        Id = Guid.NewGuid();
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
    }
}
