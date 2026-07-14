using MediatR;
using NovaERP.Application.Features.Invoicing.Common;
using NovaERP.Domain.Invoicing;

namespace NovaERP.Application.Features.Invoicing.RegisterPayment;

public sealed record RegisterPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    DateOnly PaidAt,
    PaymentMethod Method,
    string? Reference) : IRequest<InvoiceDetail>;
