using FluentValidation;
using NovaERP.Domain.Partners;

namespace NovaERP.Application.Features.Partners.UpdatePartner;

public sealed class UpdatePartnerCommandValidator : AbstractValidator<UpdatePartnerCommand>
{
    public UpdatePartnerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEqual((PartnerType)0).WithMessage("Elige si es cliente, proveedor o ambos.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.DocumentNumber).MaximumLength(50);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Address).MaximumLength(300);
    }
}
