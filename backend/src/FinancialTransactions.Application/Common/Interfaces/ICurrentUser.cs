namespace FinancialTransactions.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }

    Guid? CustomerId { get; }

    bool IsAdmin { get; }

    string CorrelationId { get; }

    string? IpAddress { get; }
}
