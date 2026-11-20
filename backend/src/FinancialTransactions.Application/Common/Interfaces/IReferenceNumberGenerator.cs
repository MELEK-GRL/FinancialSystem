namespace FinancialTransactions.Application.Common.Interfaces;

public interface IReferenceNumberGenerator
{
    Task<string> NextAsync(CancellationToken cancellationToken);
}
