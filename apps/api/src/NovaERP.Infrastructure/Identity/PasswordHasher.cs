using Microsoft.AspNetCore.Identity;
using NovaERP.Application.Features.Authentication.Common;
using DomainUser = NovaERP.Domain.Identity.User;

namespace NovaERP.Infrastructure.Identity;

/// <summary>
/// Envuelve el PasswordHasher&lt;T&gt; de ASP.NET Core (PBKDF2 con salt por hash,
/// iteraciones y formato versionado). Usamos solo el hasher, sin arrastrar todo
/// el framework de ASP.NET Core Identity que no encaja con nuestro flujo JWT.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<DomainUser> _inner = new();

    // Hash válido de una password fija que nadie usa: solo existe para que
    // Verify(hash: null) haga el mismo trabajo de CPU que un intento real.
    private readonly Lazy<string> _dummyHash = new(() => new PasswordHasher<DomainUser>().HashPassword(null!, "no-such-account-timing-guard"));

    public string Hash(string password) => _inner.HashPassword(null!, password);

    public bool Verify(string password, string? hash)
    {
        var result = _inner.VerifyHashedPassword(null!, hash ?? _dummyHash.Value, password);
        return hash is not null && result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
