namespace NovaERP.Application.Features.Invoicing.Common;

public sealed record InvoiceLineDto(
    string ProductSku, string ProductName, decimal Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record PaymentDto(Guid Id, decimal Amount, DateOnly PaidAt, string Method, string? Reference);

/// <summary>Vista de lista: lo justo para la tabla de facturas.</summary>
public sealed record InvoiceSummary(
    Guid Id,
    string InvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    decimal Total,
    decimal AmountPaid,
    decimal OutstandingBalance);

/// <summary>Vista de detalle: la factura completa con líneas y pagos.</summary>
public sealed record InvoiceDetail(
    Guid Id,
    string InvoiceNumber,
    Guid SalesOrderId,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    decimal Total,
    decimal AmountPaid,
    decimal OutstandingBalance,
    string? Notes,
    IReadOnlyList<InvoiceLineDto> Lines,
    IReadOnlyList<PaymentDto> Payments);
