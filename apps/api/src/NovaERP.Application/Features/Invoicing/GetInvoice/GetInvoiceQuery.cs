using MediatR;
using NovaERP.Application.Features.Invoicing.Common;

namespace NovaERP.Application.Features.Invoicing.GetInvoice;

public sealed record GetInvoiceQuery(Guid InvoiceId) : IRequest<InvoiceDetail>;
