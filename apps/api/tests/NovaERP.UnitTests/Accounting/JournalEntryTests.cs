using FluentAssertions;
using NovaERP.Domain.Accounting;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.UnitTests.Accounting;

public sealed class JournalEntryTests
{
    private static JournalEntry NewEntry() =>
        new(Guid.NewGuid(), "ASI-00001", new DateOnly(2026, 7, 14), "Venta al contado", null);

    [Fact]
    public void A_balanced_entry_passes_and_sets_total()
    {
        var entry = NewEntry();
        entry.AddLine(new JournalEntryLine(Guid.NewGuid(), 1000, 0));
        entry.AddLine(new JournalEntryLine(Guid.NewGuid(), 0, 1000));

        entry.EnsureBalanced();

        entry.Total.Should().Be(1000);
    }

    [Fact]
    public void An_unbalanced_entry_is_rejected()
    {
        var entry = NewEntry();
        entry.AddLine(new JournalEntryLine(Guid.NewGuid(), 1000, 0));
        entry.AddLine(new JournalEntryLine(Guid.NewGuid(), 0, 900));

        var act = entry.EnsureBalanced;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_entry_with_a_single_line_is_rejected()
    {
        var entry = NewEntry();
        entry.AddLine(new JournalEntryLine(Guid.NewGuid(), 1000, 0));

        var act = entry.EnsureBalanced;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_entry_with_no_debit_side_is_rejected()
    {
        var entry = NewEntry();
        entry.AddLine(new JournalEntryLine(Guid.NewGuid(), 0, 500));
        entry.AddLine(new JournalEntryLine(Guid.NewGuid(), 0, 500));

        var act = entry.EnsureBalanced;

        act.Should().Throw<DomainException>();
    }
}

public sealed class JournalEntryLineTests
{
    [Fact]
    public void A_line_must_charge_debit_or_credit_but_not_both()
    {
        var act = () => new JournalEntryLine(Guid.NewGuid(), 500, 500);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_line_must_charge_at_least_one_side()
    {
        var act = () => new JournalEntryLine(Guid.NewGuid(), 0, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_line_rejects_negative_amounts()
    {
        var act = () => new JournalEntryLine(Guid.NewGuid(), -100, 0);

        act.Should().Throw<DomainException>();
    }
}
