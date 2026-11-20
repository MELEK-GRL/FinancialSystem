using MediatR;

namespace FinancialTransactions.Application.Accounts.Commands.OpenAccount;

public sealed record OpenAccountCommand : IRequest<AccountDto>;
