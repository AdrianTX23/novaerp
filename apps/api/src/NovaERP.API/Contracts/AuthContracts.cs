using NovaERP.Application.Features.Authentication.Common;

namespace NovaERP.API.Contracts;

public sealed record RegisterRequest(string CompanyName, string FullName, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// El refresh token NO viaja en el body: va en una cookie httpOnly. El cliente
/// solo recibe el access token (que guarda en memoria) y los datos del usuario.
/// </summary>
public sealed record AuthResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAt, AuthUser User);
