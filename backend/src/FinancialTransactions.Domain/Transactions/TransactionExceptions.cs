using FinancialTransactions.Domain.Common;

namespace FinancialTransactions.Domain.Transactions;

public sealed class AmountLimitExceededException() : BusinessRuleException(ErrorCodes.AmountLimitExceeded);
