using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Catalog.Common;

namespace NovaERP.Application.Features.Catalog.AdjustStock;

public sealed class AdjustStockCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AdjustStockCommand, ProductSummary>
{
    public async Task<ProductSummary> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new ConflictException("El producto no existe.");

        product.AdjustStock(request.Delta);
        await db.SaveChangesAsync(ct);

        return await ProductSummaryFactory.CreateAsync(db, product, ct);
    }
}
