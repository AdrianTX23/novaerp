using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Features.Dashboard.Common;
using NovaERP.Application.Features.Dashboard.GetDashboard;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.ReportsRead)]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetDashboardQuery(), ct));
    }
}
