using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Contracts.Transactions;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Transactions;

public class DepositTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/v1/transactions/deposit";

    [Fact]
    public async Task Deposit_IncreasesBalance_RecordsSuccess_AndQueuesCompletedEvent()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var response = await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 250.75m, description = "Maaş" });
        var transaction = await response.Content.ReadFromJsonAsync<JsonElement>();
        var referenceNo = transaction.GetProperty("referenceNo").GetString()!;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/v1/transactions/{referenceNo}", response.Headers.Location?.OriginalString);
        Assert.Equal("Success", transaction.GetProperty("status").GetString());
        Assert.Equal("Deposit", transaction.GetProperty("type").GetString());
        Assert.Contains("\"description\":\"Maaş\"", await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.String, transaction.GetProperty("completedAt").ValueKind);
        Assert.Equal(250.75m, await customer.BalanceAsync(accountId));

        await using var dbContext = factory.CreateDbContext();
        var saved = await dbContext.Transactions.SingleAsync(t => t.ReferenceNo == referenceNo);
        var message = await dbContext.OutboxMessages.SingleAsync(m => m.AggregateId == saved.Id);
        var payload = JsonSerializer.Deserialize<TransactionEvent>(message.Payload, JsonSerializerOptions.Web)!;

        Assert.Equal(TransactionStatus.Success, saved.Status);
        Assert.Equal(TransactionEventTypes.Completed, message.EventType);
        Assert.Equal(250.75m, payload.Amount);
        Assert.Equal(customer.CustomerId, payload.TargetCustomerId);
        Assert.Null(payload.SourceAccountId);
    }

    [Fact]
    public async Task MultipleDeposits_AddUp()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 0.10m });
        await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 0.20m });

        Assert.Equal(0.30m, await customer.BalanceAsync(accountId));
    }

    [Fact]
    public async Task ConcurrentDepositsToSameAccount_LoseNoUpdate()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ =>
            customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 1m })));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        Assert.Equal(20m, await customer.BalanceAsync(accountId));
    }

    [Fact]
    public async Task DepositToAnotherCustomersAccount_Returns404_AndCreatesNoTransaction()
    {
        var owner = await factory.RegisterCustomerAsync();
        var stranger = await factory.RegisterCustomerAsync();
        var accountId = await owner.OpenAccountAsync();

        var response = await stranger.Client.PostAsJsonAsync(Url, new { accountId, amount = 10m });

        await using var dbContext = factory.CreateDbContext();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(await dbContext.Transactions.AnyAsync(t => t.TargetAccountId == accountId));
    }

    [Fact]
    public async Task DepositToPassiveAccount_Returns422_RecordsFailure_AndKeepsBalance()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        var admin = await factory.CreateAdminClientAsync();
        await admin.PatchAsJsonAsync($"/api/v1/accounts/{accountId}/status", new { status = "Passive", reason = "Şüpheli işlem" });

        var response = await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 10m });

        await AssertFailureRecordedAsync(response, accountId, ErrorCodes.AccountInactive);
        Assert.Equal(0m, await customer.BalanceAsync(accountId));
    }

    [Fact]
    public async Task DepositByPassiveCustomer_Returns422_AndRecordsFailure()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        var admin = await factory.CreateAdminClientAsync();
        await admin.PatchAsJsonAsync($"/api/v1/customers/{customer.CustomerId}/status", new { status = "Passive" });

        var response = await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 10m });

        await AssertFailureRecordedAsync(response, accountId, ErrorCodes.CustomerInactive);
    }

    [Fact]
    public async Task DepositAboveLimit_Returns422_AndRecordsFailure()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var response = await customer.Client.PostAsJsonAsync(Url, new { accountId, amount = 100000.01m });

        await AssertFailureRecordedAsync(response, accountId, ErrorCodes.AmountLimitExceeded);
        Assert.Equal(0m, await customer.BalanceAsync(accountId));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("10.005")]
    public async Task InvalidAmount_Returns400_AndCreatesNoTransaction(string amount)
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var response = await customer.Client.PostAsync(Url, JsonContent.Create(
            JsonDocument.Parse($"{{\"accountId\":\"{accountId}\",\"amount\":{amount}}}").RootElement));

        await using var dbContext = factory.CreateDbContext();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(await dbContext.Transactions.AnyAsync(t => t.TargetAccountId == accountId));
    }

    [Fact]
    public async Task Admin_CannotDeposit()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync(Url, new { accountId = Guid.NewGuid(), amount = 10m });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task AssertFailureRecordedAsync(HttpResponseMessage response, Guid accountId, string errorCode)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(errorCode, problem.GetProperty("errorCode").GetString());

        await using var dbContext = factory.CreateDbContext();
        var failed = await dbContext.Transactions.SingleAsync(t => t.TargetAccountId == accountId);
        Assert.Equal(TransactionStatus.Failed, failed.Status);
        Assert.Equal(errorCode, failed.FailureReason);
        Assert.NotNull(failed.CompletedAt);
        Assert.Equal(
            [TransactionEventTypes.Failed],
            await dbContext.OutboxMessages.Where(m => m.AggregateId == failed.Id).Select(m => m.EventType).ToListAsync());
    }
}
