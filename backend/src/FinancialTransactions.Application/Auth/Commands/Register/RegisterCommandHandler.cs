using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Application.Users;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Users;
using MediatR;

namespace FinancialTransactions.Application.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    ICustomerRepository customers,
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(command.FirstName, command.LastName, command.NationalId, command.Email, command.Phone);

        if (await customers.ExistsAsync(customer.NationalId, customer.Email, cancellationToken)
            || await users.EmailExistsAsync(customer.Email, cancellationToken))
        {
            throw new DuplicateCustomerException();
        }

        var user = User.CreateCustomerUser(customer.Email, passwordHasher.Hash(command.Password), customer.Id);

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(_ =>
            {
                customers.Add(customer);
                users.Add(user);
                return Task.CompletedTask;
            }, cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            throw new DuplicateCustomerException();
        }

        return new RegisterResult(customer.Id, customer.CustomerNo);
    }
}
