using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Authentication.Common;

namespace NovaERP.Application.Features.Authentication.Login;

public sealed class LoginCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // IgnoreQueryFilters: request sin autenticar, el tenant se descubre por el email.
        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        // Verificamos el hash incluso si el usuario no existe no es necesario aquí:
        // el mensaje es genérico para no revelar si el email está registrado.
        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Email o contraseña incorrectos.");
        }

        var (roles, permissions) = await PermissionLoader.LoadAsync(db, user.Id, ct);

        return await AuthResultFactory.CreateAsync(db, tokenService, user, roles, permissions, ct);
    }
}
