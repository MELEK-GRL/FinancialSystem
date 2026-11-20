namespace FinancialTransactions.Application.Transactions.Queries;

public enum TransactionHistorySort
{
    CreatedAtDesc,
    CreatedAtAsc,
    AmountDesc,
    AmountAsc,
}

public static class TransactionHistorySorts
{
    public static readonly IReadOnlyDictionary<string, TransactionHistorySort> ByName =
        new Dictionary<string, TransactionHistorySort>(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAt_desc"] = TransactionHistorySort.CreatedAtDesc,
            ["createdAt_asc"] = TransactionHistorySort.CreatedAtAsc,
            ["amount_desc"] = TransactionHistorySort.AmountDesc,
            ["amount_asc"] = TransactionHistorySort.AmountAsc,
        };

    public static TransactionHistorySort Parse(string? name) =>
        name is null ? TransactionHistorySort.CreatedAtDesc : ByName[name];
}
