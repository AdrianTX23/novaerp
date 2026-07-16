using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Invoicing.Common;
using NovaERP.Domain.Invoicing;

namespace NovaERP.Application.Features.Invoicing.ListInvoices;

public sealed class ListInvoicesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListInvoicesQuery, PagedResult<InvoiceSummary>>
{
    public async Task<PagedResult<InvoiceSummary>> Handle(ListInvoicesQuery request, CancellationToken ct)
    {
        var query = db.Invoices.AsQueryable();

        if (request.CustomerId is { } customerId)
        {
            query = query.Where(i => i.CustomerId == customerId);
        }

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<InvoiceStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(i => i.Status == status);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(i => i.IssueDate)
            .ThenByDescending(i => i.InvoiceNumber)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InvoiceSummary(
                i.Id,
                i.InvoiceNumber,
                i.CustomerId,
                i.CustomerName,
                i.Status.ToString(),
                i.IssueDate,
                i.DueDate,
                i.Total,
                i.AmountPaid,
                i.Total - i.AmountPaid))
            .ToListAsync(ct);

        return new PagedResult<InvoiceSummary>(items, totalCount);
    }
}
