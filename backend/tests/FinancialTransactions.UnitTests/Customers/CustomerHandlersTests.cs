using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Application.Customers.Commands;
using FinancialTransactions.Application.Users;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Users;
using NSubstitute;

namespace FinancialTransactions.UnitTests.Customers;

public class CustomerHandlersTests
{
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly ICustomerRepository customers = Substitute.For<ICustomerRepository>();
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly Customer customer = Customer.Create("Ayşe", "Yılmaz", "12345678950", "ayse@example.com", "5321234567");
    private readonly User user;

    public CustomerHandlersTests()
    {
        user = User.CreateCustomerUser(customer.Email, "hash", customer.Id);
        currentUser.CustomerId.Returns(customer.Id);
        customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        users.GetByCustomerIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(user);
        unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));
    }

    [Fact]
    public async Task UpdateMyProfile_ChangesCustomerAndLoginEmailTogether()
    {
        var handler = new UpdateMyProfileCommandHandler(currentUser, customers, users, unitOfWork);

        await handler.Handle(new UpdateMyProfileCommand(" New@Example.com ", "5559998877"), CancellationToken.None);

        Assert.Equal("new@example.com", customer.Email);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("5559998877", customer.Phone);
    }

    [Fact]
    public async Task UpdateMyProfile_WithTakenEmail_ThrowsDuplicate_AndChangesNothing()
    {
        users.EmailExistsAsync("taken@example.com", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new UpdateMyProfileCommandHandler(currentUser, customers, users, unitOfWork);

        await Assert.ThrowsAsync<DuplicateCustomerException>(() =>
            handler.Handle(new UpdateMyProfileCommand("taken@example.com", "5559998877"), CancellationToken.None));

        Assert.Equal("ayse@example.com", customer.Email);
    }

    [Fact]
    public async Task UpdateMyProfile_KeepingSameEmail_DoesNotCheckDuplicates()
    {
        var handler = new UpdateMyProfileCommandHandler(currentUser, customers, users, unitOfWork);

        await handler.Handle(new UpdateMyProfileCommand("ayse@example.com", "5559998877"), CancellationToken.None);

        await users.DidNotReceive().EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeStatus_UnknownCustomer_ThrowsNotFound()
    {
        var handler = new ChangeCustomerStatusCommandHandler(customers, unitOfWork);

        await Assert.ThrowsAsync<CustomerNotFoundException>(() =>
            handler.Handle(new ChangeCustomerStatusCommand(Guid.NewGuid(), CustomerStatus.Passive), CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_DeactivatesAndActivates()
    {
        var handler = new ChangeCustomerStatusCommandHandler(customers, unitOfWork);

        await handler.Handle(new ChangeCustomerStatusCommand(customer.Id, CustomerStatus.Passive), CancellationToken.None);
        Assert.False(customer.IsActive);

        await handler.Handle(new ChangeCustomerStatusCommand(customer.Id, CustomerStatus.Active), CancellationToken.None);
        Assert.True(customer.IsActive);
    }
}
