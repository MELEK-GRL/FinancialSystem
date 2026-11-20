using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Customers;
using MediatR;

namespace FinancialTransactions.Application.Accounts.Commands.OpenAccount;

public sealed class OpenAccountCommandHandler(
    ICurrentUser currentUser,
    ICustomerRepository customers,
    IAccountRepository accounts,
    IUnitOfWork unitOfWork) : IRequestHandler<OpenAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(OpenAccountCommand command, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(currentUser.RequireCustomerId(), cancellationToken)
            ?? throw new CustomerNotFoundException();

        if (!customer.IsActive)
        {
            throw new CustomerInactiveException();
        }

        var account = Account.Open(customer.Id);

        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            accounts.Add(account);
            return Task.CompletedTask;
        }, cancellationToken);

        return AccountDto.From(account);
    }
}
