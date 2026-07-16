using FluentValidation;

namespace NovaERP.Application.Features.Invoicing.CreateInvoice;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.SalesOrderId).NotEmpty().WithMessage("Indica el pedido a facturar.");
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DueDate is not null)
            .WithMessage("La fecha de vencimiento no puede ser anterior a hoy.");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
