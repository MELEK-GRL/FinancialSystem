using FinancialTransactions.Domain.Common;

namespace FinancialTransactions.Domain.Transactions;

public sealed class AmountLimitExceededException() : BusinessRuleException(ErrorCodes.AmountLimitExceeded);

public sealed class SameAccountTransferException() : BusinessRuleException(ErrorCodes.SameAccountTransfer);

public sealed class TransactionFailedException(string errorCode) : BusinessRuleException(errorCode);

public sealed class IdempotencyConflictException() : ConflictException(ErrorCodes.IdempotencyConflict);

public sealed class IdempotencyInProgressException() : ConflictException(ErrorCodes.IdempotencyInProgress);
