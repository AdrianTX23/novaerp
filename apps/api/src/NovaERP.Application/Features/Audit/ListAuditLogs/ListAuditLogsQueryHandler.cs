using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Audit.Common;

namespace NovaERP.Application.Features.Audit.ListAuditLogs;

public sealed class ListAuditLogsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListAuditLogsQuery, AuditLogPageDto>
{
    public async Task<AuditLogPageDto> Handle(ListAuditLogsQuery request, CancellationToken ct)
    {
        var query = db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            query = query.Where(a => a.EntityName == request.EntityName);
        }

        if (request.From is { } from)
        {
            var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(a => a.CreatedAt >= fromUtc);
        }

        if (request.To is { } to)
        {
            var toUtcExclusive = new DateTimeOffset(to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(a => a.CreatedAt < toUtcExclusive);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id, a.EntityName, a.EntityId, a.Action.ToString(), a.UserId, a.UserEmail, a.Changes, a.CreatedAt))
            .ToListAsync(ct);

        return new AuditLogPageDto(items, totalCount);
    }
}
