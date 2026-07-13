using MediatR;
using NovaERP.Application.Features.Authentication.Common;

namespace NovaERP.Application.Features.Authentication.Register;

/// <summary>Registro self-service: crea la empresa y su primer usuario (Owner).</summary>
public sealed record RegisterCommand(
    string CompanyName,
    string FullName,
    string Email,
    string Password) : IRequest<AuthResult>;
