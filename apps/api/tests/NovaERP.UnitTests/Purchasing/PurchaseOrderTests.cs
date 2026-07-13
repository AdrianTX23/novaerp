using FluentAssertions;
using NovaERP.Domain.Common.Exceptions;
using NovaERP.Domain.Purchasing;

namespace NovaERP.UnitTests.Purchasing;

public sealed class PurchaseOrderTests
{
    private static PurchaseOrder NewDraft() =>
        new(Guid.NewGuid(), "PO-00001", Guid.NewGuid(), new DateOnly(2026, 7, 13), null);

    private static PurchaseOrderLine Line(decimal qty = 2, decimal cost = 5) =>
        new(Guid.NewGuid(), "SKU-1", "Insumo", qty, cost);

    [Fact]
    public void New_order_starts_as_draft()
    {
        NewDraft().Status.Should().Be(PurchaseOrderStatus.Draft);
    }

    [Fact]
    public void AddLine_accumulates_the_total()
    {
        var order = NewDraft();

        order.AddLine(Line(qty: 10, cost: 5)); // 50
        order.AddLine(Line(qty: 2, cost: 2.5m)); // 5

        order.TotalAmount.Should().Be(55m);
        order.Lines.Should().HaveCount(2);
    }

    [Fact]
    public void Confirm_moves_a_draft_with_lines_to_confirmed()
    {
        var order = NewDraft();
        order.AddLine(Line());

        order.Confirm();

        order.Status.Should().Be(PurchaseOrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_without_lines_throws()
    {
        var act = NewDraft().Confirm;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirm_twice_throws()
    {
        var order = NewDraft();
        order.AddLine(Line());
        order.Confirm();

        var act = order.Confirm;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_twice_throws()
    {
        var order = NewDraft();
        order.Cancel();

        var act = order.Cancel;

        act.Should().Throw<DomainException>();
    }
}

public sealed class PurchaseOrderLineTests
{
    [Fact]
    public void Computes_line_total_from_quantity_and_cost()
    {
        var line = new PurchaseOrderLine(Guid.NewGuid(), "SKU-1", "Insumo", quantity: 100, unitCost: 5m);

        line.LineTotal.Should().Be(500m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rejects_a_non_positive_quantity(decimal quantity)
    {
        var act = () => new PurchaseOrderLine(Guid.NewGuid(), "SKU-1", "Insumo", quantity, 5);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rejects_a_negative_cost()
    {
        var act = () => new PurchaseOrderLine(Guid.NewGuid(), "SKU-1", "Insumo", 1, -5);

        act.Should().Throw<DomainException>();
    }
}
