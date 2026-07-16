namespace NovaERP.Application.Common.Interfaces;

/// <summary>
/// Numeración correlativa por tenant y tipo de documento (SO-00001, PO-00001,
/// INV-00001), libre de condiciones de carrera: dos requests concurrentes del
/// mismo tenant nunca reciben el mismo número.
/// </summary>
public interface IDocumentSequenceService
{
    Task<int> NextAsync(Guid tenantId, string documentType, CancellationToken ct);
}
