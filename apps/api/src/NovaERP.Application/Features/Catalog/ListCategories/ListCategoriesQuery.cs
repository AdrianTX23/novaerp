using MediatR;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.ListCategories;

public sealed record ListCategoriesQuery : IRequest<List<CategoryDto>>;
