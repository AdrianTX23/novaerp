namespace NovaERP.Application.Features.Crm.Common;

public sealed record OpportunityDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Title,
    decimal EstimatedValue,
    string Stage,
    DateOnly? ExpectedCloseDate,
    string? Notes);

public sealed record StageSummary(string Stage, int Count, decimal Value);

/// <summary>
/// Resumen del pipeline: valor abierto (etapas no cerradas), ganado del mes y el
/// desglose por etapa para las columnas del tablero.
/// </summary>
public sealed record PipelineSummaryDto(
    decimal OpenValue,
    int OpenCount,
    decimal WonThisMonth,
    IReadOnlyList<StageSummary> ByStage);
