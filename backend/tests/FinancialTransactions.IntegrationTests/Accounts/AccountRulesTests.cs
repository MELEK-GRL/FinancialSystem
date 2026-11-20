using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;

namespace FinancialTransactions.IntegrationTests.Accounts;

public class AccountRulesTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task ConcurrentAccountOpenings_GetDistinctAccountNumbers()
    {
        var customer = await factory.RegisterCustomerAsync();

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => customer.Client.PostAsync("/api/v1/accounts", null)));
        var accountNumbers = await Task.WhenAll(responses.Select(async response =>
            (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accountNo").GetString()));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        Assert.Equal(10, accountNumbers.Distinct().Count());
    }

    [Fact]
    public async Task PassiveCustomer_CanStillViewAccounts()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await OpenAccountAsync(customer);
        var admin = await factory.CreateAdminClientAsync();
        await admin.PatchAsJsonAsync($"/api/v1/customers/{customer.CustomerId}/status", new { status = "Passive" });

        var list = await customer.Client.GetAsync("/api/v1/accounts");
        var balance = await customer.Client.GetAsync($"/api/v1/accounts/{accountId}/balance");

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.OK, balance.StatusCode);
    }

    [Fact]
    public async Task PassiveAccount_IsShownAsInactiveInLookup()
    {
        var owner = await factory.RegisterCustomerAsync();
        var sender = await factory.RegisterCustomerAsync();
        var account = await (await owner.Client.PostAsync("/api/v1/accounts", null)).Content.ReadFromJsonAsync<JsonElement>();
        var admin = await factory.CreateAdminClientAsync();
        await admin.PatchAsJsonAsync($"/api/v1/accounts/{account.GetProperty("id").GetGuid()}/status", new { status = "Passive", reason = "Şüpheli işlem" });

        var lookup = await sender.Client.GetFromJsonAsync<JsonElement>($"/api/v1/accounts/lookup?accountNo={account.GetProperty("accountNo").GetString()}");

        Assert.False(lookup.GetProperty("isActive").GetBoolean());
    }

    [Theory]
    [InlineData("customers")]
    [InlineData("accounts")]
    public async Task InvalidStatusValue_Returns400(string resource)
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PatchAsJsonAsync($"/api/v1/{resource}/{Guid.NewGuid()}/status", new { status = "Frozen", reason = "Geçersiz durum" });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ErrorCodes.ValidationError, problem.GetProperty("errorCode").GetString());
    }

    [Theory]
    [InlineData("customers", ErrorCodes.CustomerNotFound)]
    [InlineData("accounts", ErrorCodes.AccountNotFound)]
    public async Task ChangingStatusOfUnknownResource_Returns404(string resource, string errorCode)
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PatchAsJsonAsync($"/api/v1/{resource}/{Guid.NewGuid()}/status", new { status = "Passive", reason = "Şüpheli işlem" });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(errorCode, problem.GetProperty("errorCode").GetString());
    }

    private static async Task<Guid> OpenAccountAsync(TestCustomer customer)
    {
        var response = await customer.Client.PostAsync("/api/v1/accounts", null);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }
}
