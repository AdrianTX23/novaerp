using MediatR;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.UpdateCategory;

public sealed record UpdateCategoryCommand(Guid CategoryId, string Name, string? Description) : IRequest<CategoryDto>;
