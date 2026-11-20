using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Application.Accounts.Commands.ChangeAccountStatus;
using FinancialTransactions.Application.Audit;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Audit;
using NSubstitute;

namespace FinancialTransactions.UnitTests.Accounts;

public class ChangeAccountStatusCommandHandlerTests
{
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly IAccountRepository accounts = Substitute.For<IAccountRepository>();
    private readonly IAuditLogRepository auditLogs = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly Account account = Account.Open(Guid.NewGuid());
    private readonly Guid adminId = Guid.NewGuid();
    private readonly ChangeAccountStatusCommandHandler handler;

    public ChangeAccountStatusCommandHandlerTests()
    {
        currentUser.UserId.Returns(adminId);
        currentUser.CorrelationId.Returns("corr-1");
        accounts.GetByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);
        unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));
        handler = new ChangeAccountStatusCommandHandler(currentUser, accounts, auditLogs, unitOfWork);
    }

    [Fact]
    public async Task Deactivate_ChangesStatus_AndWritesAuditWithReasonAndAdmin()
    {
        await handler.Handle(new ChangeAccountStatusCommand(account.Id, AccountStatus.Passive, " Şüpheli işlem "), CancellationToken.None);

        Assert.Equal(AccountStatus.Passive, account.Status);
        auditLogs.Received(1).Add(Arg.Is<AuditLog>(log =>
            log.AccountId == account.Id
            && log.Action == AuditAction.AccountDeactivated
            && log.OldStatus == AccountStatus.Active
            && log.NewStatus == AccountStatus.Passive
            && log.Reason == "Şüpheli işlem"
            && log.PerformedBy == adminId
            && log.CorrelationId == "corr-1"));
    }

    [Fact]
    public async Task SameStatus_ChangesNothing_AndWritesNoAudit()
    {
        await handler.Handle(new ChangeAccountStatusCommand(account.Id, AccountStatus.Active, "Zaten aktif"), CancellationToken.None);

        auditLogs.DidNotReceive().Add(Arg.Any<AuditLog>());
        await unitOfWork.DidNotReceive().ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnknownAccount_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            handler.Handle(new ChangeAccountStatusCommand(Guid.NewGuid(), AccountStatus.Passive, "Şüpheli işlem"), CancellationToken.None));
    }
}
