using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Common;
using NovaERP.Application.Features.Invoicing.Common;
using NovaERP.Application.Features.Invoicing.CreateInvoice;
using NovaERP.Application.Features.Invoicing.GetInvoice;
using NovaERP.Application.Features.Invoicing.ListInvoices;
using NovaERP.Application.Features.Invoicing.RegisterPayment;
using NovaERP.Application.Features.Invoicing.VoidInvoice;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public sealed class InvoicesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.InvoicesRead)]
    public async Task<ActionResult<PagedResult<InvoiceSummary>>> List(
        [FromQuery] string? status, [FromQuery] Guid? customerId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var query = new ListInvoicesQuery(status, customerId, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
        return Ok(await mediator.Send(query, ct));
    }

    [HttpGet("{invoiceId:guid}")]
    [Authorize(Roles = Permissions.InvoicesRead)]
    public async Task<ActionResult<InvoiceDetail>> Get(Guid invoiceId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetInvoiceQuery(invoiceId), ct));
    }

    [HttpPost]
    [Authorize(Roles = Permissions.InvoicesManage)]
    public async Task<ActionResult<InvoiceDetail>> Create(CreateInvoiceRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateInvoiceCommand(request.SalesOrderId, request.DueDate, request.Notes), ct);
        return Ok(result);
    }

    [HttpPost("{invoiceId:guid}/payments")]
    [Authorize(Roles = Permissions.InvoicesManage)]
    public async Task<ActionResult<InvoiceDetail>> RegisterPayment(
        Guid invoiceId, RegisterPaymentRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterPaymentCommand(invoiceId, request.Amount, request.PaidAt, request.Method, request.Reference), ct);
        return Ok(result);
    }

    [HttpPost("{invoiceId:guid}/void")]
    [Authorize(Roles = Permissions.InvoicesManage)]
    public async Task<ActionResult<InvoiceDetail>> Void(Guid invoiceId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new VoidInvoiceCommand(invoiceId), ct));
    }
}
