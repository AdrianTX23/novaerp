using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Accounting.Common;
using NovaERP.Application.Features.Accounting.CreateAccount;
using NovaERP.Application.Features.Accounting.CreateJournalEntry;
using NovaERP.Application.Features.Accounting.GetTrialBalance;
using NovaERP.Application.Features.Accounting.ListAccounts;
using NovaERP.Application.Features.Accounting.ListJournalEntries;
using NovaERP.Domain.Identity;

namespace NovaERP.API.Controllers;

[ApiController]
[Route("api/accounting")]
[Authorize]
public sealed class AccountingController(IMediator mediator) : ControllerBase
{
    [HttpGet("accounts")]
    [Authorize(Roles = Permissions.AccountingRead)]
    public async Task<ActionResult<List<AccountDto>>> Accounts(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListAccountsQuery(), ct));
    }

    [HttpPost("accounts")]
    [Authorize(Roles = Permissions.AccountingManage)]
    public async Task<ActionResult<AccountDto>> CreateAccount(CreateAccountRequest request, CancellationToken ct)
    {
        return Ok(await mediator.Send(new CreateAccountCommand(request.Code, request.Name, request.Type), ct));
    }

    [HttpGet("journal-entries")]
    [Authorize(Roles = Permissions.AccountingRead)]
    public async Task<ActionResult<List<JournalEntrySummary>>> JournalEntries(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListJournalEntriesQuery(), ct));
    }

    [HttpPost("journal-entries")]
    [Authorize(Roles = Permissions.AccountingManage)]
    public async Task<ActionResult<JournalEntryDetail>> CreateJournalEntry(
        CreateJournalEntryRequest request, CancellationToken ct)
    {
        var lines = request.Lines.Select(l => new JournalLineInput(l.AccountId, l.Debit, l.Credit)).ToList();
        var result = await mediator.Send(
            new CreateJournalEntryCommand(request.Date, request.Description, request.Reference, lines), ct);
        return Ok(result);
    }

    [HttpGet("trial-balance")]
    [Authorize(Roles = Permissions.AccountingRead)]
    public async Task<ActionResult<TrialBalanceDto>> TrialBalance(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetTrialBalanceQuery(), ct));
    }
}
