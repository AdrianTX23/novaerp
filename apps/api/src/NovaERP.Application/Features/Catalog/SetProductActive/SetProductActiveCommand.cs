using MediatR;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.SetProductActive;

public sealed record SetProductActiveCommand(Guid ProductId, bool IsActive) : IRequest<ProductSummary>;
