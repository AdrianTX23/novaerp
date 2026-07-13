using MediatR;
using NovaERP.Application.Features.Dashboard.Common;

namespace NovaERP.Application.Features.Dashboard.GetDashboard;

public sealed record GetDashboardQuery : IRequest<DashboardDto>;
