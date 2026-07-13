namespace NovaERP.Domain.Common.Exceptions;

/// <summary>
/// Invariante de negocio violado dentro de un agregado (ej. dejar el stock en
/// negativo). Vive en Domain porque son las entidades quienes protegen sus
/// propias reglas; ExceptionHandlingMiddleware la traduce a HTTP 409, igual
/// que ConflictException en Application.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
