using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Partners.Common;
using NovaERP.Application.Features.Partners.CreatePartner;
using NovaERP.Application.Features.Partners.ListPartners;
using NovaERP.Application.Features.Partners.SetPartnerActive;
using NovaERP.Application.Features.Partners.UpdatePartner;
using NovaERP.Domain.Identity;
using NovaERP.Domain.Partners;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/partners")]
[Authorize]
public sealed class PartnersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.PartnersRead)]
    public async Task<ActionResult<List<PartnerDto>>> List([FromQuery] PartnerType? type, CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListPartnersQuery(type), ct));
    }

    [HttpPost]
    [Authorize(Roles = Permissions.PartnersManage)]
    public async Task<ActionResult<PartnerDto>> Create(CreatePartnerRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreatePartnerCommand(
            request.Name, request.Type, request.DocumentNumber, request.Email, request.Phone, request.Address), ct);
        return Ok(result);
    }

    [HttpPut("{partnerId:guid}")]
    [Authorize(Roles = Permissions.PartnersManage)]
    public async Task<ActionResult<PartnerDto>> Update(
        Guid partnerId, UpdatePartnerRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdatePartnerCommand(
            partnerId, request.Name, request.Type, request.DocumentNumber,
            request.Email, request.Phone, request.Address), ct);
        return Ok(result);
    }

    [HttpPost("{partnerId:guid}/deactivate")]
    [Authorize(Roles = Permissions.PartnersManage)]
    public async Task<ActionResult<PartnerDto>> Deactivate(Guid partnerId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new SetPartnerActiveCommand(partnerId, IsActive: false), ct));
    }

    [HttpPost("{partnerId:guid}/reactivate")]
    [Authorize(Roles = Permissions.PartnersManage)]
    public async Task<ActionResult<PartnerDto>> Reactivate(Guid partnerId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new SetPartnerActiveCommand(partnerId, IsActive: true), ct));
    }
}
