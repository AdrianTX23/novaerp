using System.Text.Json;
using FluentValidation;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Domain.Common.Exceptions;

namespace NovaERP.API.Middleware;

/// <summary>
/// Traduce las excepciones de las capas internas a respuestas HTTP con el código
/// correcto y un cuerpo ProblemDetails. Mantiene los Handlers limpios de
/// preocupaciones HTTP y garantiza que nunca se filtre un stack trace al cliente.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Datos inválidos",
                ex.Errors.Select(e => new { field = e.PropertyName, error = e.ErrorMessage }));
        }
        catch (ConflictException ex)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);
            await WriteProblem(context, StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado.");
        }
    }

    private static async Task WriteProblem(
        HttpContext context, int statusCode, string title, object? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var payload = new
        {
            type = $"https://httpstatuses.io/{statusCode}",
            title,
            status = statusCode,
            errors,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };
}
