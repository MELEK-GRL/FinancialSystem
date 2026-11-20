using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Contracts.Transactions;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Transactions;

public class WithdrawTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/v1/transactions/withdraw";

    [Fact]
    public async Task Withdraw_DecreasesBalance_AndQueuesCompletedEvent()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 500m);

        var response = await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 120.25m, description = "ATM" });
        var transaction = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Withdrawal", transaction.GetProperty("type").GetString());
        Assert.Equal("Success", transaction.GetProperty("status").GetString());
        Assert.Equal(379.75m, await customer.BalanceAsync(accountId));

        await using var dbContext = factory.CreateDbContext();
        var message = await dbContext.OutboxMessages.SingleAsync(m => m.AggregateId == transaction.GetProperty("id").GetGuid());
        var payload = JsonSerializer.Deserialize<TransactionEvent>(message.Payload, JsonSerializerOptions.Web)!;
        Assert.Equal(TransactionEventTypes.Completed, message.EventType);
        Assert.Equal(customer.CustomerId, payload.SourceCustomerId);
        Assert.Null(payload.TargetAccountId);
    }

    [Fact]
    public async Task WithdrawingTheWholeBalance_LeavesZero()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 75.50m);

        var response = await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 75.50m });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(0m, await customer.BalanceAsync(accountId));
    }

    [Fact]
    public async Task InsufficientBalance_Returns422_RecordsFailure_AndKeepsBalance()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 100m);

        var response = await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 100.01m });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(ErrorCodes.InsufficientBalance, problem.GetProperty("errorCode").GetString());
        Assert.Equal("Yetersiz bakiye", problem.GetProperty("title").GetString());
        Assert.Equal(100m, await customer.BalanceAsync(accountId));

        await using var dbContext = factory.CreateDbContext();
        var failed = await dbContext.Transactions.SingleAsync(t => t.SourceAccountId == accountId);
        Assert.Equal(TransactionStatus.Failed, failed.Status);
        Assert.Equal(ErrorCodes.InsufficientBalance, failed.FailureReason);
        Assert.Equal(
            [TransactionEventTypes.Failed],
            await dbContext.OutboxMessages.Where(m => m.AggregateId == failed.Id).Select(m => m.EventType).ToListAsync());
    }

    [Fact]
    public async Task ConcurrentWithdrawals_NeverOverdraw()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 50m);

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ =>
            customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 10m })));

        Assert.Equal(5, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.Equal(5, responses.Count(response => response.StatusCode == HttpStatusCode.UnprocessableEntity));
        Assert.Equal(0m, await customer.BalanceAsync(accountId));
    }

    [Fact]
    public async Task WithdrawFromAnotherCustomersAccount_Returns404_AndCreatesNoTransaction()
    {
        var owner = await factory.RegisterCustomerAsync();
        var stranger = await factory.RegisterCustomerAsync();
        var accountId = await owner.OpenAccountAsync();
        await owner.DepositAsync(accountId, 100m);

        var response = await stranger.Client.PostAsJsonAsync(Url, new { accountId, amount = 10m });

        await using var dbContext = factory.CreateDbContext();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(await dbContext.Transactions.AnyAsync(t => t.SourceAccountId == accountId));
        Assert.Equal(100m, await owner.BalanceAsync(accountId));
    }

    [Fact]
    public async Task WithdrawFromPassiveAccount_Returns422()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 100m);
        var admin = await factory.CreateAdminClientAsync();
        await admin.PatchAsJsonAsync($"/api/v1/accounts/{accountId}/status", new { status = "Passive", reason = "Şüpheli işlem" });

        var response = await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 10m });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(ErrorCodes.AccountInactive, problem.GetProperty("errorCode").GetString());
        Assert.Equal(100m, await customer.BalanceAsync(accountId));
    }
}
