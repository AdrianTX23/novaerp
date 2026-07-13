using FluentAssertions;
using NovaERP.Domain.Common.Exceptions;
using NovaERP.Domain.Sales;

namespace NovaERP.UnitTests.Sales;

public sealed class SalesOrderTests
{
    private static SalesOrder NewDraft() =>
        new(Guid.NewGuid(), "SO-00001", Guid.NewGuid(), new DateOnly(2026, 7, 13), null);

    private static SalesOrderLine Line(decimal qty = 2, decimal price = 10) =>
        new(Guid.NewGuid(), "SKU-1", "Widget", qty, price);

    [Fact]
    public void New_order_starts_as_draft()
    {
        NewDraft().Status.Should().Be(SalesOrderStatus.Draft);
    }

    [Fact]
    public void AddLine_accumulates_the_total()
    {
        var order = NewDraft();

        order.AddLine(Line(qty: 2, price: 10)); // 20
        order.AddLine(Line(qty: 1, price: 5.5m)); // 5.5

        order.TotalAmount.Should().Be(25.5m);
        order.Lines.Should().HaveCount(2);
    }

    [Fact]
    public void Confirm_moves_a_draft_with_lines_to_confirmed()
    {
        var order = NewDraft();
        order.AddLine(Line());

        order.Confirm();

        order.Status.Should().Be(SalesOrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_without_lines_throws()
    {
        var order = NewDraft();

        var act = order.Confirm;

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
    public void AddLine_on_a_confirmed_order_throws()
    {
        var order = NewDraft();
        order.AddLine(Line());
        order.Confirm();

        var act = () => order.AddLine(Line());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_marks_the_order_cancelled()
    {
        var order = NewDraft();
        order.AddLine(Line());
        order.Confirm();

        order.Cancel();

        order.Status.Should().Be(SalesOrderStatus.Cancelled);
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

public sealed class SalesOrderLineTests
{
    [Fact]
    public void Computes_line_total_from_quantity_and_price()
    {
        var line = new SalesOrderLine(Guid.NewGuid(), "SKU-1", "Widget", quantity: 3, unitPrice: 12.5m);

        line.LineTotal.Should().Be(37.5m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rejects_a_non_positive_quantity(decimal quantity)
    {
        var act = () => new SalesOrderLine(Guid.NewGuid(), "SKU-1", "Widget", quantity, 10);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rejects_a_negative_price()
    {
        var act = () => new SalesOrderLine(Guid.NewGuid(), "SKU-1", "Widget", 1, -5);

        act.Should().Throw<DomainException>();
    }
}
