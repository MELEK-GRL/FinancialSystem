using FinancialTransactions.Domain.Common;

namespace FinancialTransactions.Domain.Customers;

public sealed class DuplicateCustomerException() : ConflictException(ErrorCodes.DuplicateCustomer);
