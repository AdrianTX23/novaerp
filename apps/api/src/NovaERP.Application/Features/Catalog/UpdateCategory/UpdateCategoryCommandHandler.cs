using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await db.ProductCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
            ?? throw new ConflictException("La categoría no existe.");

        var name = request.Name.Trim();
        if (await db.ProductCategories.AnyAsync(c => c.Id != category.Id && c.Name == name, ct))
        {
            throw new ConflictException("Ya existe una categoría con ese nombre.");
        }

        category.Update(name, request.Description?.Trim());
        var productCount = await db.Products.CountAsync(p => p.CategoryId == category.Id, ct);

        await db.SaveChangesAsync(ct);

        return new CategoryDto(category.Id, category.Name, category.Description, productCount);
    }
}
