using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Users;

namespace FinancialTransactions.UnitTests.Domain;

public class CustomerTests
{
    [Fact]
    public void Create_NormalizesInputAndStartsActive()
    {
        var customer = Customer.Create("  Ayşe ", " Yılmaz ", " 10000000146 ", "  Ayse@Example.COM ", " 5321234567 ");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Ayşe", customer.FirstName);
        Assert.Equal("Yılmaz", customer.LastName);
        Assert.Equal("10000000146", customer.NationalId);
        Assert.Equal("ayse@example.com", customer.Email);
        Assert.Equal("5321234567", customer.Phone);
        Assert.True(customer.IsActive);
    }

    [Fact]
    public void Deactivate_AndActivate_ChangeStatus()
    {
        var customer = Customer.Create("Ayşe", "Yılmaz", "10000000146", "ayse@example.com", "5321234567");

        customer.Deactivate();
        Assert.Equal(CustomerStatus.Passive, customer.Status);

        customer.Activate();
        Assert.Equal(CustomerStatus.Active, customer.Status);
    }

    [Fact]
    public void UpdateContact_NormalizesEmail()
    {
        var customer = Customer.Create("Ayşe", "Yılmaz", "10000000146", "ayse@example.com", "5321234567");

        customer.UpdateContact(" NEW@Example.com ", "5559876543");

        Assert.Equal("new@example.com", customer.Email);
        Assert.Equal("5559876543", customer.Phone);
    }

    [Fact]
    public void CreateCustomerUser_LinksCustomerAndRole()
    {
        var customerId = Guid.NewGuid();

        var user = User.CreateCustomerUser(" Ayse@Example.com ", "hash", customerId);

        Assert.Equal("ayse@example.com", user.Email);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.Equal(customerId, user.CustomerId);
    }

    [Fact]
    public void CreateAdmin_HasNoCustomer()
    {
        var user = User.CreateAdmin("admin@example.com", "hash");

        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Null(user.CustomerId);
    }
}
