using MediatR;
using NovaERP.Application.Features.Settings.Common;

namespace NovaERP.Application.Features.Settings.RenameCompany;

public sealed record RenameCompanyCommand(string Name) : IRequest<CompanyDto>;
