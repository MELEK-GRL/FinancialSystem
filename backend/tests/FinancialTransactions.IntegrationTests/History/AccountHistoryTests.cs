using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

    [Fact]
    public async Task Pages_DoNotOverlap_AndCoverAllTransactions()
    {
        var (customer, accountId) = await CreateAccountWithDepositsAsync(12);

        var pages = new List<JsonElement>();
        for (var page = 1; page <= 3; page++)
        {
            pages.Add(await customer.Client.GetFromJsonAsync<JsonElement>($"/api/v1/accounts/{accountId}/transactions?page={page}&size=5"));
        }

        var ids = pages.SelectMany(page => page.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("id").GetGuid())).ToList();
        Assert.Equal([5, 5, 2], pages.Select(page => page.GetProperty("items").GetArrayLength()));
        Assert.All(pages, page => Assert.Equal(12, page.GetProperty("totalCount").GetInt32()));
        Assert.Equal(3, pages[0].GetProperty("totalPages").GetInt32());
        Assert.Equal(12, ids.Distinct().Count());
    }

    [Theory]
    [InlineData("amount_asc", new[] { 1, 2, 3, 4, 5 })]
    [InlineData("amount_desc", new[] { 5, 4, 3, 2, 1 })]
    [InlineData("createdAt_asc", new[] { 1, 2, 3, 4, 5 })]
    [InlineData("createdAt_desc", new[] { 5, 4, 3, 2, 1 })]
    [InlineData("AMOUNT_ASC", new[] { 1, 2, 3, 4, 5 })]
    public async Task Sort_OrdersByAllowedFields(string sort, int[] expectedAmounts)
    {
        var (customer, accountId) = await CreateAccountWithDepositsAsync(5);

        var items = await GetItemsAsync(customer.Client, accountId, $"sort={sort}");

        Assert.Equal(expectedAmounts.Select(amount => (decimal)amount), items.Select(item => item.GetProperty("amount").GetDecimal()));
    }

    [Theory]
    [InlineData("sort=balance_desc")]
    [InlineData("sort=amount")]
    [InlineData("page=0")]
    [InlineData("size=0")]
    [InlineData("size=101")]
    public async Task InvalidPagingOrSort_Returns400(string query)
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var response = await customer.Client.GetAsync($"/api/v1/accounts/{accountId}/transactions?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvalidSort_ListsAllowedValues()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var response = await customer.Client.GetAsync($"/api/v1/accounts/{accountId}/transactions?sort=balance_desc");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        var message = problem.GetProperty("errors").EnumerateObject().Single().Value[0].GetString();
        Assert.Equal("Sıralama şunlardan biri olmalı: createdAt_desc, createdAt_asc, amount_desc, amount_asc", message);
    }

    [Fact]
    public async Task FilterAndPaging_TotalCountReflectsTheFilter()
    {
        var (customer, accountId) = await CreateAccountWithDepositsAsync(12);

        var page = await customer.Client.GetFromJsonAsync<JsonElement>(
            $"/api/v1/accounts/{accountId}/transactions?minAmount=5&sort=amount_asc&page=2&size=3");

        Assert.Equal(8, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, page.GetProperty("totalPages").GetInt32());
        Assert.Equal([8m, 9m, 10m], page.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("amount").GetDecimal()));
    }

    [Fact]
    public async Task PendingTransaction_IsVisibleInHistory()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        JsonElement pending;

        await using (var locker = factory.CreateDbContext())
        {
            await using var lockTransaction = await locker.Database.BeginTransactionAsync();
            await locker.Database.ExecuteSqlAsync($"SELECT 1 FROM accounts WHERE id = {accountId} FOR NO KEY UPDATE");

            var deposit = customer.Client.PostAsJsonAsync("/api/v1/transactions/deposit", new { accountId, amount = 10m });
            pending = await WaitForSingleAsync(customer.Client, accountId, "status=Pending");
            await lockTransaction.RollbackAsync();

            Assert.Equal(HttpStatusCode.Created, (await deposit).StatusCode);
        }

        Assert.Equal("Pending", pending.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, pending.GetProperty("completedAt").ValueKind);
        Assert.Equal("Success", Assert.Single(await GetItemsAsync(customer.Client, accountId)).GetProperty("status").GetString());
    }

    [Fact]
    public async Task PassiveAccount_HistoryIsStillVisible()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 10m);
        var admin = await factory.CreateAdminClientAsync();
        await admin.PatchAsJsonAsync($"/api/v1/accounts/{accountId}/status", new { status = "Passive", reason = "Hesap kapatma talebi" });

        Assert.Single(await GetItemsAsync(customer.Client, accountId));
    }

    [Fact]
    public async Task HistoryQuery_CanUseTheAccountIndexes()
    {
        await using var dbContext = factory.CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await dbContext.Database.ExecuteSqlRawAsync("SET LOCAL enable_seqscan = off");

        var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = """
            EXPLAIN SELECT id FROM transactions
            WHERE source_account_id = '00000000-0000-0000-0000-000000000001'
               OR target_account_id = '00000000-0000-0000-0000-000000000001'
            ORDER BY created_at DESC LIMIT 20
            """;

        var plan = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                plan.Add(reader.GetString(0));
            }
        }

        var text = string.Join('\n', plan);
        Assert.Contains("ix_transactions_source_account_id_created_at", text);
        Assert.Contains("ix_transactions_target_account_id_created_at", text);
    }

    private static async Task<JsonElement> WaitForSingleAsync(HttpClient client, Guid accountId, string filter)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (await GetItemsAsync(client, accountId, filter) is [var item])
            {
                return item;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException($"No transaction matched {filter}");
    }

    private async Task<(TestCustomer Customer, Guid AccountId)> CreateAccountWithDepositsAsync(int count)
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        for (var amount = 1; amount <= count; amount++)
        {
            await customer.DepositAsync(accountId, amount);
        }

        return (customer, accountId);
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
