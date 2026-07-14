using NovaERP.Domain.Invoicing;

namespace NovaERP.API.Contracts;

public sealed record CreateInvoiceRequest(Guid SalesOrderId, DateOnly? DueDate, string? Notes);

public sealed record RegisterPaymentRequest(
    decimal Amount, DateOnly PaidAt, PaymentMethod Method, string? Reference);
