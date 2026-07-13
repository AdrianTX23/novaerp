using MediatR;
using NovaERP.Application.Features.Authentication.Common;

namespace NovaERP.Application.Features.Authentication.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;
