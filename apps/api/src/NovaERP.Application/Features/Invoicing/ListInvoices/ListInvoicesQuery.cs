using MediatR;
using NovaERP.Application.Common;
using NovaERP.Application.Features.Invoicing.Common;

namespace NovaERP.Application.Features.Invoicing.ListInvoices;

public sealed record ListInvoicesQuery(string? Status = null, Guid? CustomerId = null, int Page = 1, int PageSize = 50)
    : IRequest<PagedResult<InvoiceSummary>>;
