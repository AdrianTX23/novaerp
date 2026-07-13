using MediatR;
using NovaERP.Application.Features.Users.Common;

namespace NovaERP.Application.Features.Users.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string FullName,
    string Password,
    List<Guid> RoleIds) : IRequest<UserSummary>;
