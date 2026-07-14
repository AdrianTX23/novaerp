using FluentAssertions;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Features.Crm.CreateOpportunity;
using NovaERP.Application.Features.Crm.GetPipelineSummary;
using NovaERP.Application.Features.Crm.MoveOpportunity;
using NovaERP.Domain.Crm;
using NovaERP.Domain.Partners;
using NovaERP.Infrastructure.Persistence;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Crm;

public sealed class CrmHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    private async Task<Guid> SeedCustomerAsync(NovaErpDbContext db)
    {
        var customer = new Partner(_tenantId, "ACME", PartnerType.Customer);
        db.Partners.Add(customer);
        await db.SaveChangesAsync(CancellationToken.None);
        return customer.Id;
    }

    private CreateOpportunityCommand NewOpp(Guid customerId, string title, decimal value) =>
        new(customerId, title, value, null, null);

    [Fact]
    public async Task Rejects_an_opportunity_for_a_non_customer()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var supplier = new Partner(_tenantId, "Prov", PartnerType.Supplier);
        db.Partners.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None);

        var act = () => new CreateOpportunityCommandHandler(db, new FakeTenantProvider(_tenantId))
            .Handle(NewOpp(supplier.Id, "x", 100), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Pipeline_summary_splits_open_and_won()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var customer = await SeedCustomerAsync(db);
        var create = new CreateOpportunityCommandHandler(db, new FakeTenantProvider(_tenantId));
        var move = new MoveOpportunityCommandHandler(db);

        var won = await create.Handle(NewOpp(customer, "ERP", 25000), CancellationToken.None);
        await create.Handle(NewOpp(customer, "Soporte", 5000), CancellationToken.None); // queda abierta
        await move.Handle(new MoveOpportunityCommand(won.Id, OpportunityStage.Won), CancellationToken.None);

        var summary = await new GetPipelineSummaryQueryHandler(db).Handle(new GetPipelineSummaryQuery(), CancellationToken.None);

        summary.OpenValue.Should().Be(5000);
        summary.OpenCount.Should().Be(1);
        summary.WonThisMonth.Should().Be(25000);
    }

    [Fact]
    public async Task Moving_a_closed_opportunity_is_rejected()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var customer = await SeedCustomerAsync(db);
        var create = new CreateOpportunityCommandHandler(db, new FakeTenantProvider(_tenantId));
        var move = new MoveOpportunityCommandHandler(db);
        var opp = await create.Handle(NewOpp(customer, "ERP", 1000), CancellationToken.None);
        await move.Handle(new MoveOpportunityCommand(opp.Id, OpportunityStage.Lost), CancellationToken.None);

        var act = () => move.Handle(new MoveOpportunityCommand(opp.Id, OpportunityStage.Proposal), CancellationToken.None);

        await act.Should().ThrowAsync<NovaERP.Domain.Common.Exceptions.DomainException>();
    }
}
