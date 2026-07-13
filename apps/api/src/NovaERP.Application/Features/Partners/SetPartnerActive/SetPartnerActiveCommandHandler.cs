using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Partners.Common;

namespace NovaERP.Application.Features.Partners.SetPartnerActive;

public sealed class SetPartnerActiveCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SetPartnerActiveCommand, PartnerDto>
{
    public async Task<PartnerDto> Handle(SetPartnerActiveCommand request, CancellationToken ct)
    {
        var partner = await db.Partners.FirstOrDefaultAsync(p => p.Id == request.PartnerId, ct)
            ?? throw new ConflictException("El contacto no existe.");

        if (request.IsActive) partner.Reactivate(); else partner.Deactivate();
        await db.SaveChangesAsync(ct);

        return partner.ToDto();
    }
}
