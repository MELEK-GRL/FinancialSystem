using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Audit;

namespace FinancialTransactions.UnitTests.Domain;

public class AuditLogTests
{
    [Theory]
    [InlineData(AccountStatus.Active, AccountStatus.Passive, AuditAction.AccountDeactivated)]
    [InlineData(AccountStatus.Passive, AccountStatus.Active, AuditAction.AccountActivated)]
    public void ForAccountStatusChange_SetsActionFromNewStatus(AccountStatus oldStatus, AccountStatus newStatus, AuditAction expected)
    {
        var log = AuditLog.ForAccountStatusChange(Guid.NewGuid(), oldStatus, newStatus, " Sebep ", Guid.NewGuid(), "corr");

        Assert.Equal(expected, log.Action);
        Assert.Equal(oldStatus, log.OldStatus);
        Assert.Equal(newStatus, log.NewStatus);
        Assert.Equal("Sebep", log.Reason);
        Assert.NotEqual(Guid.Empty, log.Id);
    }
}
