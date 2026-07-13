using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Users.Common;
using NovaERP.Application.Features.Users.CreateUser;
using NovaERP.Application.Features.Users.ListUsers;
using NovaERP.Application.Features.Users.SetUserActive;
using NovaERP.Application.Features.Users.UpdateUserRoles;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.UsersRead)]
    public async Task<ActionResult<List<UserSummary>>> List(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListUsersQuery(), ct));
    }

    [HttpPost]
    [Authorize(Roles = Permissions.UsersManage)]
    public async Task<ActionResult<UserSummary>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateUserCommand(request.Email, request.FullName, request.Password, request.RoleIds), ct);
        return Ok(result);
    }

    [HttpPut("{userId:guid}/roles")]
    [Authorize(Roles = Permissions.UsersManage)]
    public async Task<ActionResult<UserSummary>> UpdateRoles(
        Guid userId, UpdateUserRolesRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateUserRolesCommand(userId, request.RoleIds), ct);
        return Ok(result);
    }

    [HttpPost("{userId:guid}/deactivate")]
    [Authorize(Roles = Permissions.UsersManage)]
    public async Task<ActionResult<UserSummary>> Deactivate(Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new SetUserActiveCommand(userId, IsActive: false), ct);
        return Ok(result);
    }

    [HttpPost("{userId:guid}/reactivate")]
    [Authorize(Roles = Permissions.UsersManage)]
    public async Task<ActionResult<UserSummary>> Reactivate(Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new SetUserActiveCommand(userId, IsActive: true), ct);
        return Ok(result);
    }
}
