using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Authentication.Common;
using NovaERP.Application.Features.Authentication.Login;
using NovaERP.Application.Features.Authentication.Logout;
using NovaERP.Application.Features.Authentication.Me;
using NovaERP.Application.Features.Authentication.Refresh;
using NovaERP.Application.Features.Authentication.Register;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    private const string RefreshCookieName = "novaerp_refresh";

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterCommand(request.CompanyName, request.FullName, request.Email, request.Password), ct);
        return Ok(BuildResponse(result));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        return Ok(BuildResponse(result));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(new RefreshTokenCommand(refreshToken), ct);
        return Ok(BuildResponse(result));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await mediator.Send(new LogoutCommand(refreshToken), ct);
        }

        Response.Cookies.Delete(RefreshCookieName);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUser>> Me(CancellationToken ct)
    {
        var user = await mediator.Send(new GetMeQuery(), ct);
        return Ok(user);
    }

    private AuthResponse BuildResponse(AuthResult result)
    {
        Response.Cookies.Append(RefreshCookieName, result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            // None (no Strict): en producción el frontend (Vercel) y la API (Railway/
            // Render) viven en dominios distintos, así que son "cross-site" para el
            // navegador. Una cookie Strict jamás se envía en ese caso — el refresh
            // silencioso quedaría roto en producción aunque funcione en local
            // (localhost:3000/5080 sí cuentan como "same-site" entre sí). None exige
            // Secure=true, que ya estaba puesto.
            SameSite = SameSiteMode.None,
            Expires = result.RefreshTokenExpiresAt,
            Path = "/api/auth",
        });

        return new AuthResponse(result.AccessToken, result.AccessTokenExpiresAt, result.User);
    }
}
