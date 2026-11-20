using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Application.Accounts.Commands.OpenAccount;
using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Customers;
using NSubstitute;

namespace FinancialTransactions.UnitTests.Accounts;

public class AccountHandlersTests
{
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly ICustomerRepository customers = Substitute.For<ICustomerRepository>();
    private readonly IAccountRepository accounts = Substitute.For<IAccountRepository>();
    private readonly IAccountQueries accountQueries = Substitute.For<IAccountQueries>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    public AccountHandlersTests()
    {
        unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));
    }

    [Fact]
    public async Task OpenAccount_ForActiveCustomer_AddsAccount()
    {
        var customer = Customer.Create("Ayşe", "Yılmaz", "12345678950", "ayse@example.com", "5321234567");
        currentUser.CustomerId.Returns(customer.Id);
        customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var result = await Handler().Handle(new OpenAccountCommand(), CancellationToken.None);

        Assert.Equal(customer.Id, result.CustomerId);
        accounts.Received(1).Add(Arg.Is<Account>(account => account.CustomerId == customer.Id));
    }

    [Fact]
    public async Task OpenAccount_ForPassiveCustomer_Throws()
    {
        var customer = Customer.Create("Ayşe", "Yılmaz", "12345678950", "ayse@example.com", "5321234567");
        customer.Deactivate();
        currentUser.CustomerId.Returns(customer.Id);
        customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        await Assert.ThrowsAsync<CustomerInactiveException>(() => Handler().Handle(new OpenAccountCommand(), CancellationToken.None));

        accounts.DidNotReceive().Add(Arg.Any<Account>());
    }

    [Fact]
    public async Task GetAccount_OfAnotherCustomer_ThrowsNotFound()
    {
        var account = new AccountDto(Guid.NewGuid(), "TR1000000001", Guid.NewGuid(), "TRY", 0m, AccountStatus.Active, DateTimeOffset.UtcNow);
        currentUser.CustomerId.Returns(Guid.NewGuid());
        accountQueries.GetByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            new GetAccountQueryHandler(currentUser, accountQueries).Handle(new GetAccountQuery(account.Id), CancellationToken.None));
    }

    private OpenAccountCommandHandler Handler() => new(currentUser, customers, accounts, unitOfWork);
}
