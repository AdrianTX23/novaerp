using MediatR;
using NovaERP.Application.Features.Users.Common;

namespace NovaERP.Application.Features.Users.ListUsers;

public sealed record ListUsersQuery : IRequest<List<UserSummary>>;
