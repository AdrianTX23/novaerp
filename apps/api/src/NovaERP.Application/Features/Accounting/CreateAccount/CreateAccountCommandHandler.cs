using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Exceptions;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Accounting.Common;
using NovaERP.Domain.Accounting;

namespace NovaERP.Application.Features.Accounting.CreateAccount;

public sealed class CreateAccountCommandHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken ct)
    {
        var code = request.Code.Trim();
        if (await db.Accounts.AnyAsync(a => a.Code == code, ct))
        {
            throw new ConflictException("Ya existe una cuenta con ese código.");
        }

        var account = new Account(tenantProvider.TenantId, code, request.Name, request.Type);
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);

        return new AccountDto(account.Id, account.Code, account.Name, account.Type.ToString(), account.IsSystem);
    }
}
