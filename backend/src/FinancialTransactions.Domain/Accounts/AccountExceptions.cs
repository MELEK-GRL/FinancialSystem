using FinancialTransactions.Domain.Common;

namespace FinancialTransactions.Domain.Accounts;

public sealed class InsufficientBalanceException() : BusinessRuleException(ErrorCodes.InsufficientBalance);

public sealed class AccountInactiveException() : BusinessRuleException(ErrorCodes.AccountInactive);

public sealed class AccountNotFoundException() : NotFoundException(ErrorCodes.AccountNotFound);
