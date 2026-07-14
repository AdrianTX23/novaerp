using FluentValidation;

namespace NovaERP.Application.Features.Cash.CreateCashMovement;

public sealed class CreateCashMovementCommandValidator : AbstractValidator<CreateCashMovementCommand>
{
    public CreateCashMovementCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor que cero.");
        RuleFor(x => x.Concept).NotEmpty().WithMessage("Indica un concepto.").MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
