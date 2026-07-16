using MediatR;
using NovaERP.Application.Features.Audit.Common;

namespace NovaERP.Application.Features.Audit.ListAuditLogs;

public sealed record ListAuditLogsQuery(
    string? EntityName,
    DateOnly? From,
    DateOnly? To,
    int Page,
    int PageSize) : IRequest<AuditLogPageDto>;
