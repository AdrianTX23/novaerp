using FluentValidation;

namespace NovaERP.Application.Features.Catalog.AdjustStock;

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.Delta).NotEqual(0).WithMessage("El ajuste no puede ser cero.");
    }
}
