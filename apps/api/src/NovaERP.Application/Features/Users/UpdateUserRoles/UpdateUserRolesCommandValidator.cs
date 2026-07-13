using FluentValidation;

namespace NovaERP.Application.Features.Users.UpdateUserRoles;

public sealed class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(x => x.RoleIds).NotEmpty().WithMessage("El usuario debe tener al menos un rol.");
    }
}
