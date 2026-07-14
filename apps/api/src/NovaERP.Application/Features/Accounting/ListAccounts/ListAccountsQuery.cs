using MediatR;
using NovaERP.Application.Features.Accounting.Common;

namespace NovaERP.Application.Features.Accounting.ListAccounts;

public sealed record ListAccountsQuery : IRequest<List<AccountDto>>;
