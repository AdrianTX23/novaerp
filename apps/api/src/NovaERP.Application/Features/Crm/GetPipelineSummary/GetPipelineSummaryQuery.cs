using MediatR;
using NovaERP.Application.Features.Crm.Common;

namespace NovaERP.Application.Features.Crm.GetPipelineSummary;

public sealed record GetPipelineSummaryQuery : IRequest<PipelineSummaryDto>;
