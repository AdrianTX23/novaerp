using MediatR;
using NovaERP.Application.Features.Invoicing.Common;

namespace NovaERP.Application.Features.Invoicing.VoidInvoice;

public sealed record VoidInvoiceCommand(Guid InvoiceId) : IRequest<InvoiceDetail>;
