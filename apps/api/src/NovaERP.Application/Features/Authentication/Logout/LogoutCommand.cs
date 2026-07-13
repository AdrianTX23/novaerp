using MediatR;

namespace NovaERP.Application.Features.Authentication.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;
