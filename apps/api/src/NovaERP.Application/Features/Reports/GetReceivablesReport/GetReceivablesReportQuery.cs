using MediatR;
using NovaERP.Application.Features.Reports.Common;

namespace NovaERP.Application.Features.Reports.GetReceivablesReport;

public sealed record GetReceivablesReportQuery : IRequest<ReceivablesReportDto>;
