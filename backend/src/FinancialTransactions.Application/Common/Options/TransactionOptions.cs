using System.ComponentModel.DataAnnotations;

namespace FinancialTransactions.Application.Common.Options;

public sealed class TransactionOptions
{
    public const string SectionName = "Transaction";

    [Range(typeof(decimal), "0.01", "1000000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal MaxAmount { get; init; }

    [Range(100, 30000)]
    public int LockTimeoutMilliseconds { get; init; } = 3000;
}
