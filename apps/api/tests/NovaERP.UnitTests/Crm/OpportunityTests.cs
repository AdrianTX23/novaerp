using FluentAssertions;
using NovaERP.Domain.Common.Exceptions;
using NovaERP.Domain.Crm;

namespace NovaERP.UnitTests.Crm;

public sealed class OpportunityTests
{
    private static readonly DateOnly Today = new(2026, 7, 14);

    private static Opportunity NewOpp() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Implementación ERP", 25000, null, null);

    [Fact]
    public void New_opportunity_starts_in_new_stage_and_is_open()
    {
        var opp = NewOpp();

        opp.Stage.Should().Be(OpportunityStage.New);
        opp.IsClosed.Should().BeFalse();
        opp.ClosedOn.Should().BeNull();
    }

    [Fact]
    public void Moving_through_open_stages_keeps_it_open()
    {
        var opp = NewOpp();

        opp.MoveTo(OpportunityStage.Qualified, Today);
        opp.MoveTo(OpportunityStage.Proposal, Today);

        opp.Stage.Should().Be(OpportunityStage.Proposal);
        opp.IsClosed.Should().BeFalse();
        opp.ClosedOn.Should().BeNull();
    }

    [Fact]
    public void Winning_closes_it_and_records_the_close_date()
    {
        var opp = NewOpp();

        opp.MoveTo(OpportunityStage.Won, Today);

        opp.IsClosed.Should().BeTrue();
        opp.ClosedOn.Should().Be(Today);
    }

    [Fact]
    public void A_closed_opportunity_cannot_be_moved()
    {
        var opp = NewOpp();
        opp.MoveTo(OpportunityStage.Won, Today);

        var act = () => opp.MoveTo(OpportunityStage.Proposal, Today);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_negative_estimated_value_is_rejected()
    {
        var act = () => new Opportunity(Guid.NewGuid(), Guid.NewGuid(), "x", -1, null, null);

        act.Should().Throw<DomainException>();
    }
}
