using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;

namespace NovaERP.Application.Features.Catalog.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await db.ProductCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
            ?? throw new ConflictException("La categoría no existe.");

        var hasProducts = await db.Products.AnyAsync(p => p.CategoryId == category.Id, ct);
        if (hasProducts)
        {
            throw new ConflictException("No puedes eliminar una categoría que tiene productos asignados.");
        }

        db.ProductCategories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
}
