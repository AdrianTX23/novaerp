using MediatR;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Description) : IRequest<CategoryDto>;
