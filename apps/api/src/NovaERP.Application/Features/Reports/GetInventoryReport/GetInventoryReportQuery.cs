using MediatR;
using NovaERP.Application.Features.Reports.Common;

namespace NovaERP.Application.Features.Reports.GetInventoryReport;

public sealed record GetInventoryReportQuery : IRequest<InventoryReportDto>;
