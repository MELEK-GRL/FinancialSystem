using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;

namespace FinancialTransactions.IntegrationTests.History;

public class AccountHistoryTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task History_ShowsAllStatuses_WithDirection_NewestFirst()
    {
        var customer = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var account = await customer.OpenAccountWithNumberAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await customer.DepositAsync(account.Id, 100m);
        await customer.Client.PostAsJsonAsync("/api/v1/transactions/withdraw", new { accountId = account.Id, amount = 30m });
        await customer.Client.PostAsJsonAsync("/api/v1/transactions/withdraw", new { accountId = account.Id, amount = 500m });
        await customer.Client.PostAsJsonAsync(
            "/api/v1/transactions/transfer", new { sourceAccountId = account.Id, targetAccountNo = target.AccountNo, amount = 20m });

        var items = await GetItemsAsync(customer.Client, account.Id);

        Assert.Equal(
            ["Transfer/Success/Out", "Withdrawal/Failed/Out", "Withdrawal/Success/Out", "Deposit/Success/In"],
            items.Select(item => $"{item.GetProperty("type")}/{item.GetProperty("status")}/{item.GetProperty("direction")}"));
        Assert.Equal(target.AccountNo, items[0].GetProperty("counterpartyAccountNo").GetString());
        Assert.Equal(ErrorCodes.InsufficientBalance, items[1].GetProperty("failureReason").GetString());
        Assert.Equal(JsonValueKind.Null, items[3].GetProperty("counterpartyAccountNo").ValueKind);
    }

    [Fact]
    public async Task Receiver_SeesIncomingTransfer_WithSenderAccountNo()
    {
        var sender = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var source = await sender.OpenAccountWithNumberAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await sender.DepositAsync(source.Id, 50m);
        await sender.Client.PostAsJsonAsync(
            "/api/v1/transactions/transfer", new { sourceAccountId = source.Id, targetAccountNo = target.AccountNo, amount = 15m });

        var item = Assert.Single(await GetItemsAsync(receiver.Client, target.Id));

        Assert.Equal("In", item.GetProperty("direction").GetString());
        Assert.Equal(15m, item.GetProperty("amount").GetDecimal());
        Assert.Equal(source.AccountNo, item.GetProperty("counterpartyAccountNo").GetString());
    }

    [Fact]
    public async Task TransferBetweenOwnAccounts_IsOutOnSourceAndInOnTarget()
    {
        var customer = await factory.RegisterCustomerAsync();
        var first = await customer.OpenAccountAsync();
        var second = await customer.OpenAccountWithNumberAsync();
        await customer.DepositAsync(first, 40m);
        await customer.Client.PostAsJsonAsync(
            "/api/v1/transactions/transfer", new { sourceAccountId = first, targetAccountNo = second.AccountNo, amount = 40m });

        var sourceItems = await GetItemsAsync(customer.Client, first);
        var targetItems = await GetItemsAsync(customer.Client, second.Id);

        Assert.Equal("Out", sourceItems[0].GetProperty("direction").GetString());
        Assert.Equal("In", Assert.Single(targetItems).GetProperty("direction").GetString());
    }

    [Fact]
    public async Task NewAccount_HasEmptyHistory()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var page = await customer.Client.GetFromJsonAsync<JsonElement>($"/api/v1/accounts/{accountId}/transactions");

        Assert.Equal(0, page.GetProperty("totalCount").GetInt32());
        Assert.Empty(page.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task OtherCustomer_Gets404_AndAdminCanSee()
    {
        var owner = await factory.RegisterCustomerAsync();
        var stranger = await factory.RegisterCustomerAsync();
        var accountId = await owner.OpenAccountAsync();
        await owner.DepositAsync(accountId, 10m);
        var admin = await factory.CreateAdminClientAsync();

        var strangerResponse = await stranger.Client.GetAsync($"/api/v1/accounts/{accountId}/transactions");
        var problem = await strangerResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.NotFound, strangerResponse.StatusCode);
        Assert.Equal(ErrorCodes.AccountNotFound, problem.GetProperty("errorCode").GetString());
        Assert.Single(await GetItemsAsync(admin, accountId));
    }

    [Theory]
    [InlineData("type=Withdrawal", 2)]
    [InlineData("status=Failed", 1)]
    [InlineData("type=Withdrawal&status=Success", 1)]
    [InlineData("minAmount=25&maxAmount=100", 2)]
    [InlineData("minAmount=500", 1)]
    [InlineData("maxAmount=20", 1)]
    public async Task Filters_NarrowTheHistory(string filter, int expectedCount)
    {
        var (customer, accountId) = await CreateAccountWithFourTransactionsAsync();

        var items = await GetItemsAsync(customer.Client, accountId, filter);

        Assert.Equal(expectedCount, items.Count);
    }

    [Fact]
    public async Task DateFilter_UsesIstanbulDays()
    {
        var (customer, accountId) = await CreateAccountWithFourTransactionsAsync();
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul")).DateTime);

        Assert.Equal(4, (await GetItemsAsync(customer.Client, accountId, $"from={today:yyyy-MM-dd}&to={today:yyyy-MM-dd}")).Count);
        Assert.Empty(await GetItemsAsync(customer.Client, accountId, $"from={today.AddDays(1):yyyy-MM-dd}"));
        Assert.Empty(await GetItemsAsync(customer.Client, accountId, $"to={today.AddDays(-1):yyyy-MM-dd}"));
    }

    [Theory]
    [InlineData("from=2026-11-20&to=2026-11-19")]
    [InlineData("minAmount=100&maxAmount=50")]
    [InlineData("minAmount=-5")]
    [InlineData("type=Unknown")]
    [InlineData("from=2026-13-01")]
    public async Task InvalidFilter_Returns400(string filter)
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var response = await customer.Client.GetAsync($"/api/v1/accounts/{accountId}/transactions?{filter}");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ErrorCodes.ValidationError, problem.GetProperty("errorCode").GetString());
    }

    private async Task<(TestCustomer Customer, Guid AccountId)> CreateAccountWithFourTransactionsAsync()
    {
        var customer = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await customer.DepositAsync(accountId, 100m);
        await customer.Client.PostAsJsonAsync("/api/v1/transactions/withdraw", new { accountId, amount = 30m });
        await customer.Client.PostAsJsonAsync("/api/v1/transactions/withdraw", new { accountId, amount = 500m });
        await customer.Client.PostAsJsonAsync(
            "/api/v1/transactions/transfer", new { sourceAccountId = accountId, targetAccountNo = target.AccountNo, amount = 20m });
        return (customer, accountId);
    }

    private static async Task<List<JsonElement>> GetItemsAsync(HttpClient client, Guid accountId, string filter = "")
    {
        var page = await client.GetFromJsonAsync<JsonElement>($"/api/v1/accounts/{accountId}/transactions?{filter}");
        return page.GetProperty("items").EnumerateArray().ToList();
    }
}
