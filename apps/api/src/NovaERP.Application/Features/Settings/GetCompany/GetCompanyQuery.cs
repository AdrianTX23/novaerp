using MediatR;
using NovaERP.Application.Features.Settings.Common;

namespace NovaERP.Application.Features.Settings.GetCompany;

public sealed record GetCompanyQuery : IRequest<CompanyDto>;
