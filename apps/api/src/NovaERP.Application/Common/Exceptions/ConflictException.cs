namespace NovaERP.Application.Common.Exceptions;

/// <summary>Regla de negocio violada: el recurso ya existe o el estado no lo permite (HTTP 409).</summary>
public sealed class ConflictException(string message) : Exception(message);
