using FinancialTransactions.Domain.Customers;

namespace FinancialTransactions.Application.Customers;

public sealed record CustomerDto(
    Guid Id,
    string CustomerNo,
    string FirstName,
    string LastName,
    string NationalId,
    string Email,
    string Phone,
    CustomerStatus Status,
    DateTimeOffset CreatedAt)
{
    public static CustomerDto From(Customer customer) =>
        new(customer.Id, customer.CustomerNo, customer.FirstName, customer.LastName, customer.NationalId,
            customer.Email, customer.Phone, customer.Status, customer.CreatedAt);
}

public sealed record CustomerSummaryDto(
    Guid Id,
    string CustomerNo,
    string FirstName,
    string LastName,
    string Email,
    CustomerStatus Status,
    DateTimeOffset CreatedAt);
