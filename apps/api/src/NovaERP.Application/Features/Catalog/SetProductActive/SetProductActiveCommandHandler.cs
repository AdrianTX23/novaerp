using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.SetProductActive;

public sealed class SetProductActiveCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SetProductActiveCommand, ProductSummary>
{
    public async Task<ProductSummary> Handle(SetProductActiveCommand request, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new ConflictException("El producto no existe.");

        if (request.IsActive) product.Reactivate(); else product.Deactivate();
        await db.SaveChangesAsync(ct);

        return await ProductSummaryFactory.CreateAsync(db, product, ct);
    }
}
