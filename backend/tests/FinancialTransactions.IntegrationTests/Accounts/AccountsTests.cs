using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Accounts;

public class AccountsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/v1/accounts";

    [Fact]
    public async Task OpenAccount_Returns201WithLocationAndZeroBalance()
    {
        var customer = await factory.RegisterCustomerAsync();

        var response = await customer.Client.PostAsync(Url, null);
        var account = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/v1/accounts/{account.GetProperty("id").GetGuid()}", response.Headers.Location?.AbsolutePath);
        Assert.Matches("^TR[0-9]{10}$", account.GetProperty("accountNo").GetString());
        Assert.Equal(0m, account.GetProperty("balance").GetDecimal());
        Assert.Equal("Active", account.GetProperty("status").GetString());
        Assert.Equal(customer.CustomerId, account.GetProperty("customerId").GetGuid());
    }

    [Fact]
    public async Task ListAccounts_ReturnsOnlyOwnAccounts()
    {
        var customer = await factory.RegisterCustomerAsync();
        var other = await factory.RegisterCustomerAsync();
        await customer.Client.PostAsync(Url, null);
        await customer.Client.PostAsync(Url, null);
        await other.Client.PostAsync(Url, null);

        var accounts = await customer.Client.GetFromJsonAsync<JsonElement>(Url);

        Assert.Equal(2, accounts.GetArrayLength());
        Assert.All(accounts.EnumerateArray(), account =>
            Assert.Equal(customer.CustomerId, account.GetProperty("customerId").GetGuid()));
    }

    [Fact]
    public async Task GetAccountAndBalance_WorkForOwner()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await OpenAccountAsync(customer);

        var account = await customer.Client.GetAsync($"{Url}/{accountId}");
        var balance = await customer.Client.GetFromJsonAsync<JsonElement>($"{Url}/{accountId}/balance");

        Assert.Equal(HttpStatusCode.OK, account.StatusCode);
        Assert.Equal(0m, balance.GetProperty("balance").GetDecimal());
        Assert.Equal("TRY", balance.GetProperty("currency").GetString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("/balance")]
    public async Task OtherCustomersAccount_Returns404(string suffix)
    {
        var owner = await factory.RegisterCustomerAsync();
        var stranger = await factory.RegisterCustomerAsync();
        var accountId = await OpenAccountAsync(owner);

        var response = await stranger.Client.GetAsync($"{Url}/{accountId}{suffix}");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ErrorCodes.AccountNotFound, problem.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Admin_ListsAccountsOfGivenCustomer_AndMustProvideCustomerId()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await OpenAccountAsync(customer);
        var admin = await factory.CreateAdminClientAsync();

        var accounts = await admin.GetFromJsonAsync<JsonElement>($"{Url}?customerId={customer.CustomerId}");
        var withoutCustomer = await admin.GetAsync(Url);
        var single = await admin.GetAsync($"{Url}/{accountId}");

        Assert.Equal(accountId, accounts[0].GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.BadRequest, withoutCustomer.StatusCode);
        Assert.Equal(HttpStatusCode.OK, single.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotOpenAccount()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsync(Url, null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PassiveCustomer_CannotOpenAccount()
    {
        var customer = await factory.RegisterCustomerAsync();
        await using (var dbContext = factory.CreateDbContext())
        {
            var entity = await dbContext.Customers.SingleAsync(c => c.Id == customer.CustomerId);
            entity.Deactivate();
            await dbContext.SaveChangesAsync();
        }

        var response = await customer.Client.PostAsync(Url, null);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(ErrorCodes.CustomerInactive, problem.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Anonymous_Gets401()
    {
        var response = await factory.CreateClient().GetAsync(Url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<Guid> OpenAccountAsync(TestCustomer customer)
    {
        var response = await customer.Client.PostAsync(Url, null);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }
}
