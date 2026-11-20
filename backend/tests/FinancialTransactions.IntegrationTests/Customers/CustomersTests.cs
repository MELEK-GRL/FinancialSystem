using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;

namespace FinancialTransactions.IntegrationTests.Customers;

public class CustomersTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Customer_ReadsOwnProfile()
    {
        var customer = await factory.RegisterCustomerAsync();

        var profile = await customer.Client.GetFromJsonAsync<JsonElement>("/api/v1/customers/me");

        Assert.Equal(customer.CustomerId, profile.GetProperty("id").GetGuid());
        Assert.Equal(customer.Email, profile.GetProperty("email").GetString());
        Assert.Equal("Active", profile.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Customer_UpdatesContact_AndCanLogInWithNewEmail()
    {
        var customer = await factory.RegisterCustomerAsync();
        var newEmail = TestData.UniqueEmail();

        var response = await customer.Client.PatchAsJsonAsync("/api/v1/customers/me", new { email = newEmail.ToUpperInvariant(), phone = "5559998877" });
        var profile = await response.Content.ReadFromJsonAsync<JsonElement>();
        var login = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new { email = newEmail, password = TestAuth.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(newEmail, profile.GetProperty("email").GetString());
        Assert.Equal("5559998877", profile.GetProperty("phone").GetString());
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Customer_CannotTakeAnotherCustomersEmail()
    {
        var customer = await factory.RegisterCustomerAsync();
        var other = await factory.RegisterCustomerAsync();

        var response = await customer.Client.PatchAsJsonAsync("/api/v1/customers/me", new { email = other.Email, phone = "5559998877" });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ErrorCodes.DuplicateCustomer, problem.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Admin_SearchesCustomersByEmail_WithPaging()
    {
        var customer = await factory.RegisterCustomerAsync();
        var admin = await factory.CreateAdminClientAsync();

        var page = await admin.GetFromJsonAsync<JsonElement>($"/api/v1/customers?search={customer.Email.ToUpperInvariant()}&page=1&size=10");

        Assert.Equal(1, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(customer.CustomerId, page.GetProperty("items")[0].GetProperty("id").GetGuid());
        Assert.False(page.GetProperty("items")[0].TryGetProperty("nationalId", out _));
    }

    [Fact]
    public async Task Admin_SearchTreatsWildcardsAsText()
    {
        await factory.RegisterCustomerAsync();
        var admin = await factory.CreateAdminClientAsync();

        var page = await admin.GetFromJsonAsync<JsonElement>("/api/v1/customers?search=%25");

        Assert.Equal(0, page.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Admin_DeactivatesCustomer_ThenStatusFilterAndAccountRuleApply()
    {
        var customer = await factory.RegisterCustomerAsync();
        var admin = await factory.CreateAdminClientAsync();

        var change = await admin.PatchAsJsonAsync($"/api/v1/customers/{customer.CustomerId}/status", new { status = "Passive" });
        var passive = await admin.GetFromJsonAsync<JsonElement>($"/api/v1/customers?search={customer.Email}&status=Passive");
        var openAccount = await customer.Client.PostAsync("/api/v1/accounts", null);

        Assert.Equal(HttpStatusCode.OK, change.StatusCode);
        Assert.Equal(1, passive.GetProperty("totalCount").GetInt32());
        Assert.Equal(HttpStatusCode.UnprocessableEntity, openAccount.StatusCode);
    }

    [Fact]
    public async Task Admin_GetsCustomerDetail_And404ForUnknown()
    {
        var customer = await factory.RegisterCustomerAsync();
        var admin = await factory.CreateAdminClientAsync();

        var detail = await admin.GetFromJsonAsync<JsonElement>($"/api/v1/customers/{customer.CustomerId}");
        var unknown = await admin.GetAsync($"/api/v1/customers/{Guid.NewGuid()}");

        Assert.Equal(11, detail.GetProperty("nationalId").GetString()!.Length);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/customers?page=0")]
    [InlineData("/api/v1/customers?size=101")]
    [InlineData("/api/v1/customers?status=Unknown")]
    public async Task Admin_InvalidListParameters_Return400(string url)
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RolesCannotUseEachOthersEndpoints()
    {
        var customer = await factory.RegisterCustomerAsync();
        var admin = await factory.CreateAdminClientAsync();

        var customerListsCustomers = await customer.Client.GetAsync("/api/v1/customers");
        var adminReadsMe = await admin.GetAsync("/api/v1/customers/me");

        Assert.Equal(HttpStatusCode.Forbidden, customerListsCustomers.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, adminReadsMe.StatusCode);
    }
}
