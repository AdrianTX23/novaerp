using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Accounting.Common;

namespace NovaERP.Application.Features.Accounting.ListAccounts;

public sealed class ListAccountsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListAccountsQuery, List<AccountDto>>
{
    public async Task<List<AccountDto>> Handle(ListAccountsQuery request, CancellationToken ct)
    {
        return await db.Accounts
            .OrderBy(a => a.Code)
            .Select(a => new AccountDto(a.Id, a.Code, a.Name, a.Type.ToString(), a.IsSystem))
            .ToListAsync(ct);
    }
}
