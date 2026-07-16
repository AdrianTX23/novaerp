using FluentValidation;

namespace NovaERP.Application.Features.Settings.RenameCompany;

public sealed class RenameCompanyCommandValidator : AbstractValidator<RenameCompanyCommand>
{
    public RenameCompanyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
            .MaximumLength(200);
    }
}
