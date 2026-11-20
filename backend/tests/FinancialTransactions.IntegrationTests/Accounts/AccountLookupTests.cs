using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Accounts;

public class AccountLookupTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Lookup_ReturnsMaskedOwnerName_AndNothingSensitive()
    {
        var owner = await factory.RegisterCustomerAsync();
        var sender = await factory.RegisterCustomerAsync();
        var accountNo = await OpenAccountAsync(owner);

        var response = await sender.Client.GetAsync($"/api/v1/accounts/lookup?accountNo={accountNo.ToLowerInvariant()}");
        var body = await response.Content.ReadAsStringAsync();
        var lookup = JsonDocument.Parse(body).RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(accountNo, lookup.GetProperty("accountNo").GetString());
        Assert.Equal("Ay*** Yı***", lookup.GetProperty("ownerName").GetString());
        Assert.True(lookup.GetProperty("isActive").GetBoolean());
        Assert.DoesNotContain("balance", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(owner.CustomerId.ToString(), body);
    }

    [Fact]
    public async Task Lookup_ReportsInactive_WhenOwnerIsPassive()
    {
        var owner = await factory.RegisterCustomerAsync();
        var sender = await factory.RegisterCustomerAsync();
        var accountNo = await OpenAccountAsync(owner);
        await using (var dbContext = factory.CreateDbContext())
        {
            (await dbContext.Customers.SingleAsync(c => c.Id == owner.CustomerId)).Deactivate();
            await dbContext.SaveChangesAsync();
        }

        var lookup = await sender.Client.GetFromJsonAsync<JsonElement>($"/api/v1/accounts/lookup?accountNo={accountNo}");

        Assert.False(lookup.GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task Lookup_UnknownAccount_Returns404()
    {
        var sender = await factory.RegisterCustomerAsync();

        var response = await sender.Client.GetAsync("/api/v1/accounts/lookup?accountNo=TR9999999999");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ErrorCodes.AccountNotFound, problem.GetProperty("errorCode").GetString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("TR123")]
    [InlineData("DE1000000001")]
    public async Task Lookup_InvalidAccountNo_Returns400(string accountNo)
    {
        var sender = await factory.RegisterCustomerAsync();

        var response = await sender.Client.GetAsync($"/api/v1/accounts/lookup?accountNo={accountNo}");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.GetProperty("errors").TryGetProperty("accountNo", out _));
    }

    [Fact]
    public async Task Lookup_IsNotAvailableToAdmin()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.GetAsync("/api/v1/accounts/lookup?accountNo=TR1000000001");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<string> OpenAccountAsync(TestCustomer customer)
    {
        var response = await customer.Client.PostAsync("/api/v1/accounts", null);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accountNo").GetString()!;
    }
}
