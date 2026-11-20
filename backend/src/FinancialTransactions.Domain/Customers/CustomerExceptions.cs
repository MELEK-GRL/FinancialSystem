using FinancialTransactions.Domain.Common;

namespace FinancialTransactions.Domain.Customers;

public sealed class DuplicateCustomerException() : ConflictException(ErrorCodes.DuplicateCustomer);

public sealed class CustomerInactiveException() : BusinessRuleException(ErrorCodes.CustomerInactive);

public sealed class CustomerNotFoundException() : NotFoundException(ErrorCodes.CustomerNotFound);
