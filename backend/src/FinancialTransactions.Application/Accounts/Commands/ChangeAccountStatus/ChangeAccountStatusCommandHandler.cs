using FinancialTransactions.Application.Audit;
using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Audit;
using MediatR;

namespace FinancialTransactions.Application.Accounts.Commands.ChangeAccountStatus;

public sealed class ChangeAccountStatusCommandHandler(
    ICurrentUser currentUser,
    IAccountRepository accounts,
    IAuditLogRepository auditLogs,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeAccountStatusCommand, AccountDto>
{
    public async Task<AccountDto> Handle(ChangeAccountStatusCommand command, CancellationToken cancellationToken)
    {
        var adminId = currentUser.UserId ?? throw new ForbiddenAccessException();

        return await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var account = (await accounts.GetForUpdateAsync([command.AccountId], token)).SingleOrDefault()
                ?? throw new AccountNotFoundException();
            var oldStatus = account.Status;

            if (oldStatus == command.Status)
            {
                return AccountDto.From(account);
            }

            if (command.Status == AccountStatus.Active)
            {
                account.Activate();
            }
            else
            {
                account.Deactivate();
            }

            auditLogs.Add(AuditLog.ForAccountStatusChange(
                account.Id, oldStatus, command.Status, command.Reason, adminId, currentUser.CorrelationId));

            return AccountDto.From(account);
        }, cancellationToken);
    }
}
