using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Settings.Common;

namespace NovaERP.Application.Features.Settings.RenameCompany;

public sealed class RenameCompanyCommandHandler(
    IApplicationDbContext db,
    ITenantProvider tenantProvider) : IRequestHandler<RenameCompanyCommand, CompanyDto>
{
    public async Task<CompanyDto> Handle(RenameCompanyCommand request, CancellationToken ct)
    {
        var tenant = await db.Tenants
            .SingleAsync(t => t.Id == tenantProvider.TenantId, ct);

        tenant.Rename(request.Name.Trim());
        await db.SaveChangesAsync(ct);

        return new CompanyDto(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, tenant.CreatedAt);
    }
}
