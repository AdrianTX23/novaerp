using MediatR;
using NovaERP.Application.Features.Accounting.Common;

namespace NovaERP.Application.Features.Accounting.GetTrialBalance;

public sealed record GetTrialBalanceQuery : IRequest<TrialBalanceDto>;
