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
        var account = await accounts.GetByIdAsync(command.AccountId, cancellationToken) ?? throw new AccountNotFoundException();
        var oldStatus = account.Status;

        if (oldStatus == command.Status)
        {
            return AccountDto.From(account);
        }

        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
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

            return Task.CompletedTask;
        }, cancellationToken);

        return AccountDto.From(account);
    }
}
