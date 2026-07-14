using MediatR;
using NovaERP.Application.Features.Accounting.Common;
using NovaERP.Domain.Accounting;

namespace NovaERP.Application.Features.Accounting.CreateAccount;

public sealed record CreateAccountCommand(string Code, string Name, AccountType Type) : IRequest<AccountDto>;
