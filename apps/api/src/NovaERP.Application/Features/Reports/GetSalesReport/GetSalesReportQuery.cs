using MediatR;
using NovaERP.Application.Features.Reports.Common;

namespace NovaERP.Application.Features.Reports.GetSalesReport;

public sealed record GetSalesReportQuery(DateOnly From, DateOnly To) : IRequest<SalesReportDto>;
