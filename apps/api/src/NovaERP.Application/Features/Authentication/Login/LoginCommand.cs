using MediatR;
using NovaERP.Application.Features.Authentication.Common;

namespace NovaERP.Application.Features.Authentication.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
