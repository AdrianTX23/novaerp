using MediatR;
using NovaERP.Application.Features.Invoicing.Common;

namespace NovaERP.Application.Features.Invoicing.CreateInvoice;

/// <summary>Emite una factura desde un pedido de venta confirmado. DueDate opcional
/// (por defecto, 30 días desde la emisión).</summary>
public sealed record CreateInvoiceCommand(Guid SalesOrderId, DateOnly? DueDate, string? Notes)
    : IRequest<InvoiceDetail>;
