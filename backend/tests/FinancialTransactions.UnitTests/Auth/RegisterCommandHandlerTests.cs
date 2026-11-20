using FinancialTransactions.Application.Auth.Commands.Register;
using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Application.Users;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Users;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FinancialTransactions.UnitTests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly ICustomerRepository customers = Substitute.For<ICustomerRepository>();
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RegisterCommandHandler handler;

    public RegisterCommandHandlerTests()
    {
        passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");
        unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));

        handler = new RegisterCommandHandler(customers, users, passwordHasher, unitOfWork);
    }

    [Fact]
    public async Task NewCustomer_AddsCustomerAndUserWithHashedPassword()
    {
        var result = await handler.Handle(Command(), CancellationToken.None);

        customers.Received(1).Add(Arg.Is<Customer>(customer => customer.Id == result.CustomerId && customer.Email == "ayse@example.com"));
        users.Received(1).Add(Arg.Is<User>(user =>
            user.CustomerId == result.CustomerId && user.PasswordHash == "hashed-password" && user.Role == UserRole.Customer));
    }

    [Fact]
    public async Task ExistingNationalIdOrEmail_ThrowsDuplicate_AndSavesNothing()
    {
        customers.ExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<DuplicateCustomerException>(() => handler.Handle(Command(), CancellationToken.None));

        customers.DidNotReceive().Add(Arg.Any<Customer>());
        users.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task UniqueViolationFromDatabase_IsReportedAsDuplicate()
    {
        unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new UniqueConstraintViolationException("ix_customers_national_id", new Exception()));

        await Assert.ThrowsAsync<DuplicateCustomerException>(() => handler.Handle(Command(), CancellationToken.None));
    }

    private static RegisterCommand Command() =>
        new("Ayşe", "Yılmaz", "12345678950", " Ayse@Example.com ", "5321234567", "Sifre1234");
}
