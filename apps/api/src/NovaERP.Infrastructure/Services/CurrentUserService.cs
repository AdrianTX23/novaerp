using Microsoft.AspNetCore.Http;
using NovaERP.Application.Common.Interfaces;

namespace NovaERP.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
            return Guid.TryParse(claim, out var userId) ? userId : null;
        }
    }

    public string? Email =>
        httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value;
}
