using MediatR;
using NovaERP.Application.Features.Cash.Common;

namespace NovaERP.Application.Features.Cash.GetCashSummary;

public sealed record GetCashSummaryQuery : IRequest<CashSummaryDto>;
