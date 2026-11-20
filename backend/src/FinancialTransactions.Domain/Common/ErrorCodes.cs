namespace FinancialTransactions.Domain.Common;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string Forbidden = "FORBIDDEN";
    public const string TooManyRequests = "TOO_MANY_REQUESTS";
    public const string InternalError = "INTERNAL_ERROR";

    public const string CustomerNotFound = "CUSTOMER_NOT_FOUND";
    public const string AccountNotFound = "ACCOUNT_NOT_FOUND";
    public const string AccountInactive = "ACCOUNT_INACTIVE";
    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
    public const string SameAccountTransfer = "SAME_ACCOUNT_TRANSFER";
    public const string AmountLimitExceeded = "AMOUNT_LIMIT_EXCEEDED";
    public const string DuplicateCustomer = "DUPLICATE_CUSTOMER";
    public const string IdempotencyConflict = "IDEMPOTENCY_CONFLICT";
}
