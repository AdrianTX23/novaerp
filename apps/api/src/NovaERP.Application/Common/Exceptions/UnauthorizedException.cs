namespace NovaERP.Application.Common.Exceptions;

/// <summary>Credenciales inválidas o token no válido (HTTP 401).</summary>
public sealed class UnauthorizedException(string message) : Exception(message);
