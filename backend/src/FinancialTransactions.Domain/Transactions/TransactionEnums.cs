namespace FinancialTransactions.Domain.Transactions;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
}

public enum TransactionStatus
{
    Pending,
    Success,
    Failed,
}
