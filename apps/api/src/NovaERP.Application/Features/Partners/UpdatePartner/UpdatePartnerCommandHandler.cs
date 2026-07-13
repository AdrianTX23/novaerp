using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Partners.Common;

namespace NovaERP.Application.Features.Partners.UpdatePartner;

public sealed class UpdatePartnerCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdatePartnerCommand, PartnerDto>
{
    public async Task<PartnerDto> Handle(UpdatePartnerCommand request, CancellationToken ct)
    {
        var partner = await db.Partners.FirstOrDefaultAsync(p => p.Id == request.PartnerId, ct)
            ?? throw new ConflictException("El contacto no existe.");

        partner.Update(
            request.Name, request.Type, request.DocumentNumber?.Trim(),
            request.Email?.Trim(), request.Phone?.Trim(), request.Address?.Trim());

        await db.SaveChangesAsync(ct);

        return partner.ToDto();
    }
}
