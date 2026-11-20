namespace FinancialTransactions.Application.Common.Exceptions;

public sealed class LockTimeoutException(Exception? innerException = null)
    : Exception("Timed out waiting for a row lock", innerException);
