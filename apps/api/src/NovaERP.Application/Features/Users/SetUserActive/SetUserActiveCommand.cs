using MediatR;
using NovaERP.Application.Features.Users.Common;

namespace NovaERP.Application.Features.Users.SetUserActive;

public sealed record SetUserActiveCommand(Guid UserId, bool IsActive) : IRequest<UserSummary>;
