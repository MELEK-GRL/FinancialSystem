namespace FinancialTransactions.Application.Transactions.Queries;

public interface ITransactionQueries
{
    Task<TransactionWithOwners?> GetByReferenceNoAsync(string referenceNo, CancellationToken cancellationToken);
}

public sealed record TransactionWithOwners(
    TransactionDto Transaction,
    Guid InitiatedByCustomerId,
    Guid? SourceCustomerId,
    Guid? TargetCustomerId)
{
    public IEnumerable<Guid> InvolvedCustomerIds =>
        new[] { InitiatedByCustomerId, SourceCustomerId, TargetCustomerId }.OfType<Guid>();
}
