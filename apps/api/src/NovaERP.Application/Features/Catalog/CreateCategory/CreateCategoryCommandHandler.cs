using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Catalog.Common;
using NovaERP.Domain.Catalog;

namespace NovaERP.Application.Features.Catalog.CreateCategory;

public sealed class CreateCategoryCommandHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        if (await db.ProductCategories.AnyAsync(c => c.Name == name, ct))
        {
            throw new ConflictException("Ya existe una categoría con ese nombre.");
        }

        var category = new ProductCategory(tenantProvider.TenantId, name, request.Description?.Trim());
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(ct);

        return new CategoryDto(category.Id, category.Name, category.Description, 0);
    }
}
