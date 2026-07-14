using FluentValidation;

namespace NovaERP.Application.Features.Crm.CreateOpportunity;

public sealed class CreateOpportunityCommandValidator : AbstractValidator<CreateOpportunityCommand>
{
    public CreateOpportunityCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Selecciona un cliente.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Indica un título.").MaximumLength(200);
        RuleFor(x => x.EstimatedValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
