using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using NSubstitute;

namespace FinancialTransactions.UnitTests.Common.Security;

public class CurrentUserExtensionsTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public void Customer_CanAccessOwnResource()
    {
        Assert.True(Customer(OwnerId).CanAccessCustomer(OwnerId));
    }

    [Fact]
    public void Customer_CannotAccessAnotherCustomersResource()
    {
        Assert.False(Customer(Guid.NewGuid()).CanAccessCustomer(OwnerId));
    }

    [Fact]
    public void Admin_CanAccessAnyResource()
    {
        var admin = Substitute.For<ICurrentUser>();
        admin.IsAdmin.Returns(true);

        Assert.True(admin.CanAccessCustomer(OwnerId));
    }

    [Fact]
    public void RequireCustomerId_ReturnsId_ForCustomer()
    {
        Assert.Equal(OwnerId, Customer(OwnerId).RequireCustomerId());
    }

    [Fact]
    public void RequireCustomerId_Throws_WhenUserHasNoCustomer()
    {
        var admin = Substitute.For<ICurrentUser>();
        admin.IsAdmin.Returns(true);

        Assert.Throws<ForbiddenAccessException>(() => admin.RequireCustomerId());
    }

    private static ICurrentUser Customer(Guid customerId)
    {
        var user = Substitute.For<ICurrentUser>();
        user.CustomerId.Returns(customerId);
        return user;
    }
}
