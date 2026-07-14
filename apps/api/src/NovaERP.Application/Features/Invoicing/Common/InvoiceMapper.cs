using NovaERP.Domain.Invoicing;

namespace NovaERP.Application.Features.Invoicing.Common;

/// <summary>Convierte una Invoice cargada (con líneas y pagos) a sus DTOs.</summary>
public static class InvoiceMapper
{
    public static InvoiceDetail ToDetail(Invoice invoice) => new(
        invoice.Id,
        invoice.InvoiceNumber,
        invoice.SalesOrderId,
        invoice.CustomerId,
        invoice.CustomerName,
        invoice.Status.ToString(),
        invoice.IssueDate,
        invoice.DueDate,
        invoice.Total,
        invoice.AmountPaid,
        invoice.OutstandingBalance,
        invoice.Notes,
        invoice.Lines
            .Select(l => new InvoiceLineDto(l.ProductSku, l.ProductName, l.Quantity, l.UnitPrice, l.LineTotal))
            .ToList(),
        invoice.Payments
            .OrderBy(p => p.PaidAt)
            .Select(p => new PaymentDto(p.Id, p.Amount, p.PaidAt, p.Method.ToString(), p.Reference))
            .ToList());
}
