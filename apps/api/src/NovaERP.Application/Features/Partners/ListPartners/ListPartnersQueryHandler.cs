using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Partners.Common;

namespace NovaERP.Application.Features.Partners.ListPartners;

public sealed class ListPartnersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListPartnersQuery, List<PartnerDto>>
{
    public async Task<List<PartnerDto>> Handle(ListPartnersQuery request, CancellationToken ct)
    {
        var query = db.Partners.AsQueryable();

        if (request.Type is { } type)
        {
            // Flags: un Partner "Ambos" debe aparecer tanto al filtrar por Customer
            // como por Supplier, de ahí el AND bit a bit en vez de igualdad.
            query = query.Where(p => (p.Type & type) == type);
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new PartnerDto(
                p.Id, p.Name, p.Type, p.DocumentNumber, p.Email, p.Phone, p.Address, p.IsActive))
            .ToListAsync(ct);
    }
}
