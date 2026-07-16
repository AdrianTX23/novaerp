using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Partners.Common;

namespace NovaERP.Application.Features.Partners.ListPartners;

public sealed class ListPartnersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListPartnersQuery, PagedResult<PartnerDto>>
{
    public async Task<PagedResult<PartnerDto>> Handle(ListPartnersQuery request, CancellationToken ct)
    {
        var query = db.Partners.AsQueryable();

        if (request.Type is { } type)
        {
            // Flags: un Partner "Ambos" debe aparecer tanto al filtrar por Customer
            // como por Supplier, de ahí el AND bit a bit en vez de igualdad.
            query = query.Where(p => (p.Type & type) == type);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PartnerDto(
                p.Id, p.Name, p.Type, p.DocumentNumber, p.Email, p.Phone, p.Address, p.IsActive))
            .ToListAsync(ct);

        return new PagedResult<PartnerDto>(items, totalCount);
    }
}
