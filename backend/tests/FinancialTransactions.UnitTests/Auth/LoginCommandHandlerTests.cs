using FinancialTransactions.Application.Auth.Commands.Login;
using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Users;
using FinancialTransactions.Domain.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FinancialTransactions.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService tokenService = Substitute.For<IJwtTokenService>();
    private readonly LoginCommandHandler handler;

    public LoginCommandHandlerTests()
    {
        handler = new LoginCommandHandler(users, passwordHasher, tokenService, NullLogger<LoginCommandHandler>.Instance);
    }

    [Fact]
    public async Task UnknownEmail_ThrowsInvalidCredentials_AndStillChecksPassword()
    {
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.Handle(new LoginCommand("nobody@example.com", "Sifre1234"), CancellationToken.None));

        passwordHasher.Received(1).Verify("Sifre1234", null);
    }

    [Fact]
    public async Task WrongPassword_ThrowsInvalidCredentials()
    {
        var user = User.CreateCustomerUser("ayse@example.com", "stored-hash", Guid.NewGuid());
        users.GetByEmailAsync("ayse@example.com", Arg.Any<CancellationToken>()).Returns(user);
        passwordHasher.Verify("wrong", "stored-hash").Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.Handle(new LoginCommand("ayse@example.com", "wrong"), CancellationToken.None));

        tokenService.DidNotReceive().Create(Arg.Any<User>());
    }

    [Fact]
    public async Task CorrectPassword_ReturnsTokenAndRole_WithNormalizedEmail()
    {
        var user = User.CreateCustomerUser("ayse@example.com", "stored-hash", Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        users.GetByEmailAsync("ayse@example.com", Arg.Any<CancellationToken>()).Returns(user);
        passwordHasher.Verify("Sifre1234", "stored-hash").Returns(true);
        tokenService.Create(user).Returns(new AccessToken("jwt-token", expiresAt));

        var result = await handler.Handle(new LoginCommand(" Ayse@Example.com ", "Sifre1234"), CancellationToken.None);

        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal(expiresAt, result.ExpiresAt);
        Assert.Equal(AppRoles.Customer, result.Role);
    }
}
