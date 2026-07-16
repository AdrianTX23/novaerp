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

        // Verify() se llama siempre, exista o no el usuario (con hash: null
        // internamente compara contra un hash dummy) — así el tiempo de
        // respuesta no delata si un email está registrado.
        var passwordValid = passwordHasher.Verify(request.Password, user?.PasswordHash);
        if (user is null || !user.IsActive || !passwordValid)
        {
            throw new UnauthorizedException("Email o contraseña incorrectos.");
        }

        // Si el framework subió los parámetros de PBKDF2 desde que este hash se
        // generó, es el único momento con la password en claro para regenerarlo
        // — sin esto, los usuarios quedan para siempre en el costo de hash con
        // el que se registraron.
        if (passwordHasher.NeedsRehash(request.Password, user.PasswordHash))
        {
            user.UpdatePasswordHash(passwordHasher.Hash(request.Password));
            await db.SaveChangesAsync(ct);
        }

        var (roles, permissions) = await PermissionLoader.LoadAsync(db, user.Id, ct);

        return await AuthResultFactory.CreateAsync(db, tokenService, user, roles, permissions, ct);
    }
}
