namespace FinancialTransactions.Domain.Common;

public abstract class DomainException(string errorCode) : Exception(errorCode)
{
    public string ErrorCode { get; } = errorCode;
}

public abstract class NotFoundException(string errorCode) : DomainException(errorCode);

public abstract class ConflictException(string errorCode) : DomainException(errorCode);

public abstract class BusinessRuleException(string errorCode) : DomainException(errorCode);
