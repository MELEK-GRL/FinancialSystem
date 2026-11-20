namespace FinancialTransactions.Application.Common.Exceptions;

public sealed class UniqueConstraintViolationException(string? constraintName, Exception innerException)
    : Exception($"Unique constraint violated: {constraintName}", innerException)
{
    public string? ConstraintName { get; } = constraintName;
}
