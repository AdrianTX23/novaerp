using MediatR;
using NovaERP.Application.Features.Invoicing.Common;

namespace NovaERP.Application.Features.Invoicing.ListInvoices;

public sealed record ListInvoicesQuery(string? Status = null, Guid? CustomerId = null)
    : IRequest<List<InvoiceSummary>>;
