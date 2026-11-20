using FinancialTransactions.Domain.Common;

namespace FinancialTransactions.Domain.Transactions;

public sealed class AmountLimitExceededException() : BusinessRuleException(ErrorCodes.AmountLimitExceeded);

public sealed class SameAccountTransferException() : BusinessRuleException(ErrorCodes.SameAccountTransfer);
