using MediatR;
using NovaERP.Application.Features.Authentication.Common;

namespace NovaERP.Application.Features.Authentication.Me;

public sealed record GetMeQuery : IRequest<AuthUser>;
