using FluentAssertions;
using NovaERP.Domain.Cash;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.UnitTests.Cash;

public sealed class CashMovementTests
{
    [Fact]
    public void Creates_a_movement_with_trimmed_concept()
    {
        var movement = new CashMovement(
            Guid.NewGuid(), CashMovementType.Expense, 400, new DateOnly(2026, 7, 14), "  Renta  ", null);

        movement.Concept.Should().Be("Renta");
        movement.Type.Should().Be(CashMovementType.Expense);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Rejects_a_non_positive_amount(decimal amount)
    {
        var act = () => new CashMovement(
            Guid.NewGuid(), CashMovementType.Income, amount, new DateOnly(2026, 7, 14), "x", null);

        act.Should().Throw<DomainException>();
    }
}
