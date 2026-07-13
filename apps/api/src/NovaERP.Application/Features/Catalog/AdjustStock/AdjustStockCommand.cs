using MediatR;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.AdjustStock;

/// <summary>Delta positivo = entrada manual; negativo = salida/merma manual.</summary>
public sealed record AdjustStockCommand(Guid ProductId, decimal Delta) : IRequest<ProductSummary>;
