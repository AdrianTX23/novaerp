using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Settings.Common;

namespace NovaERP.Application.Features.Settings.GetCompany;

/// <summary>
/// Tenant no tiene Global Query Filter (es la raíz del aislamiento), así que
/// aquí se filtra explícitamente por el tenant del JWT.
/// </summary>
public sealed class GetCompanyQueryHandler(
    IApplicationDbContext db,
    ITenantProvider tenantProvider) : IRequestHandler<GetCompanyQuery, CompanyDto>
{
    public async Task<CompanyDto> Handle(GetCompanyQuery request, CancellationToken ct)
    {
        var tenant = await db.Tenants
            .AsNoTracking()
            .SingleAsync(t => t.Id == tenantProvider.TenantId, ct);

        return new CompanyDto(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, tenant.CreatedAt);
    }
}
