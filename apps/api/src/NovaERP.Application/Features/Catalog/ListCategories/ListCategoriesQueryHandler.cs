using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.ListCategories;

public sealed class ListCategoriesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(ListCategoriesQuery request, CancellationToken ct)
    {
        return await db.ProductCategories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id, c.Name, c.Description,
                db.Products.Count(p => p.CategoryId == c.Id)))
            .ToListAsync(ct);
    }
}
