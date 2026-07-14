using FluentAssertions;
using NovaERP.Application.Features.Accounting.CreateJournalEntry;
using NovaERP.Application.Features.Accounting.GetTrialBalance;
using NovaERP.Domain.Accounting;
using NovaERP.Domain.Common.Exceptions;
using NovaERP.Infrastructure.Persistence;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Accounting;

public sealed class AccountingHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private async Task<(Guid caja, Guid ventas)> SeedAccountsAsync(NovaErpDbContext db)
    {
        var caja = new Account(_tenantId, "1000", "Caja", AccountType.Asset);
        var ventas = new Account(_tenantId, "4000", "Ventas", AccountType.Income);
        db.Accounts.AddRange(caja, ventas);
        await db.SaveChangesAsync(CancellationToken.None);
        return (caja.Id, ventas.Id);
    }

    [Fact]
    public async Task Creates_a_balanced_journal_entry()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (caja, ventas) = await SeedAccountsAsync(db);

        var result = await new CreateJournalEntryCommandHandler(db, new FakeTenantProvider(_tenantId)).Handle(
            new CreateJournalEntryCommand(Today, "Venta al contado", "F-1",
                [new JournalLineInput(caja, 1000, 0), new JournalLineInput(ventas, 0, 1000)]),
            CancellationToken.None);

        result.Number.Should().Be("ASI-00001");
        result.Total.Should().Be(1000);
        result.Lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task Rejects_an_unbalanced_journal_entry()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (caja, ventas) = await SeedAccountsAsync(db);

        var act = () => new CreateJournalEntryCommandHandler(db, new FakeTenantProvider(_tenantId)).Handle(
            new CreateJournalEntryCommand(Today, "Malo", null,
                [new JournalLineInput(caja, 1000, 0), new JournalLineInput(ventas, 0, 900)]),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Trial_balance_totals_match_and_report_is_balanced()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var (caja, ventas) = await SeedAccountsAsync(db);
        var create = new CreateJournalEntryCommandHandler(db, new FakeTenantProvider(_tenantId));
        await create.Handle(new CreateJournalEntryCommand(Today, "V1", null,
            [new JournalLineInput(caja, 1000, 0), new JournalLineInput(ventas, 0, 1000)]), CancellationToken.None);
        await create.Handle(new CreateJournalEntryCommand(Today, "V2", null,
            [new JournalLineInput(caja, 500, 0), new JournalLineInput(ventas, 0, 500)]), CancellationToken.None);

        var balance = await new GetTrialBalanceQueryHandler(db).Handle(new GetTrialBalanceQuery(), CancellationToken.None);

        balance.TotalDebit.Should().Be(1500);
        balance.TotalCredit.Should().Be(1500);
        balance.IsBalanced.Should().BeTrue();
        balance.Rows.Should().Contain(r => r.Code == "1000" && r.Balance == 1500);  // Caja (deudora)
        balance.Rows.Should().Contain(r => r.Code == "4000" && r.Balance == 1500);  // Ventas (acreedora)
    }
}
