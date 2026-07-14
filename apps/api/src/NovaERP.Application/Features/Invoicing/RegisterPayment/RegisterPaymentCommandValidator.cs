using FluentValidation;

namespace NovaERP.Application.Features.Invoicing.RegisterPayment;

public sealed class RegisterPaymentCommandValidator : AbstractValidator<RegisterPaymentCommand>
{
    public RegisterPaymentCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor que cero.");
        RuleFor(x => x.Reference).MaximumLength(200);
    }
}
