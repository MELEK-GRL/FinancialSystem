using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Contracts.Transactions;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Transactions;

public class TransferTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/v1/transactions/transfer";

    [Fact]
    public async Task Transfer_MovesMoney_AndQueuesEventForBothCustomers()
    {
        var sender = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var sourceId = await sender.OpenAccountAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await sender.DepositAsync(sourceId, 1000m);

        var response = await sender.Client.PostAsJsonAsync(Url, new
        {
            sourceAccountId = sourceId,
            targetAccountNo = $" {target.AccountNo.ToLowerInvariant()} ",
            amount = 250.50m,
            description = "Kira",
        });
        var transaction = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Transfer", transaction.GetProperty("type").GetString());
        Assert.Equal("Success", transaction.GetProperty("status").GetString());
        Assert.Equal(749.50m, await sender.BalanceAsync(sourceId));
        Assert.Equal(250.50m, await receiver.BalanceAsync(target.Id));

        await using var dbContext = factory.CreateDbContext();
        var message = await dbContext.OutboxMessages.SingleAsync(m => m.AggregateId == transaction.GetProperty("id").GetGuid());
        var payload = JsonSerializer.Deserialize<TransactionEvent>(message.Payload, JsonSerializerOptions.Web)!;
        Assert.Equal(sender.CustomerId, payload.SourceCustomerId);
        Assert.Equal(receiver.CustomerId, payload.TargetCustomerId);
    }

    [Fact]
    public async Task Transfer_BetweenOwnAccounts_IsAllowed()
    {
        var customer = await factory.RegisterCustomerAsync();
        var sourceId = await customer.OpenAccountAsync();
        var target = await customer.OpenAccountWithNumberAsync();
        await customer.DepositAsync(sourceId, 100m);

        var response = await customer.Client.PostAsJsonAsync(Url, new { sourceAccountId = sourceId, targetAccountNo = target.AccountNo, amount = 40m });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(60m, await customer.BalanceAsync(sourceId));
        Assert.Equal(40m, await customer.BalanceAsync(target.Id));
    }

    [Fact]
    public async Task InsufficientBalance_Returns422_RecordsFailure_AndMovesNothing()
    {
        var sender = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var sourceId = await sender.OpenAccountAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await sender.DepositAsync(sourceId, 50m);

        var response = await sender.Client.PostAsJsonAsync(Url, new { sourceAccountId = sourceId, targetAccountNo = target.AccountNo, amount = 50.01m });

        await AssertFailedAsync(response, sourceId, ErrorCodes.InsufficientBalance);
        Assert.Equal(50m, await sender.BalanceAsync(sourceId));
        Assert.Equal(0m, await receiver.BalanceAsync(target.Id));
    }

    [Fact]
    public async Task PassiveTargetAccount_Returns422_AndRecordsFailure()
    {
        var sender = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var sourceId = await sender.OpenAccountAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await sender.DepositAsync(sourceId, 100m);
        var admin = await factory.CreateAdminClientAsync();
        await admin.PatchAsJsonAsync($"/api/v1/accounts/{target.Id}/status", new { status = "Passive", reason = "Şüpheli işlem" });

        var response = await sender.Client.PostAsJsonAsync(Url, new { sourceAccountId = sourceId, targetAccountNo = target.AccountNo, amount = 10m });

        await AssertFailedAsync(response, sourceId, ErrorCodes.AccountInactive);
        Assert.Equal(100m, await sender.BalanceAsync(sourceId));
    }

    [Fact]
    public async Task TransferToSameAccount_Returns422_WithoutRecord()
    {
        var customer = await factory.RegisterCustomerAsync();
        var account = await customer.OpenAccountWithNumberAsync();
        await customer.DepositAsync(account.Id, 100m);

        var response = await customer.Client.PostAsJsonAsync(Url, new { sourceAccountId = account.Id, targetAccountNo = account.AccountNo, amount = 10m });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        await using var dbContext = factory.CreateDbContext();
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(ErrorCodes.SameAccountTransfer, problem.GetProperty("errorCode").GetString());
        Assert.False(await dbContext.Transactions.AnyAsync(t => t.SourceAccountId == account.Id));
    }

    [Fact]
    public async Task UnknownTargetAccount_Returns404_WithoutRecord()
    {
        var customer = await factory.RegisterCustomerAsync();
        var sourceId = await customer.OpenAccountAsync();
        await customer.DepositAsync(sourceId, 100m);

        var response = await customer.Client.PostAsJsonAsync(Url, new { sourceAccountId = sourceId, targetAccountNo = "TR9999999999", amount = 10m });

        await using var dbContext = factory.CreateDbContext();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(await dbContext.Transactions.AnyAsync(t => t.SourceAccountId == sourceId));
    }

    [Fact]
    public async Task TransferFromAnotherCustomersAccount_Returns404()
    {
        var owner = await factory.RegisterCustomerAsync();
        var thief = await factory.RegisterCustomerAsync();
        var sourceId = await owner.OpenAccountAsync();
        var target = await thief.OpenAccountWithNumberAsync();
        await owner.DepositAsync(sourceId, 100m);

        var response = await thief.Client.PostAsJsonAsync(Url, new { sourceAccountId = sourceId, targetAccountNo = target.AccountNo, amount = 100m });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(100m, await owner.BalanceAsync(sourceId));
    }

    [Fact]
    public async Task OppositeTransfersAtTheSameTime_DoNotDeadlock_AndKeepTotal()
    {
        var first = await factory.RegisterCustomerAsync();
        var second = await factory.RegisterCustomerAsync();
        var a = await first.OpenAccountWithNumberAsync();
        var b = await second.OpenAccountWithNumberAsync();
        await first.DepositAsync(a.Id, 1000m);
        await second.DepositAsync(b.Id, 1000m);

        var aToB = Enumerable.Range(0, 20).Select(_ =>
            first.Client.PostAsJsonAsync(Url, new { sourceAccountId = a.Id, targetAccountNo = b.AccountNo, amount = 10m }));
        var bToA = Enumerable.Range(0, 20).Select(_ =>
            second.Client.PostAsJsonAsync(Url, new { sourceAccountId = b.Id, targetAccountNo = a.AccountNo, amount = 10m }));
        var responses = await Task.WhenAll(aToB.Concat(bToA));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        Assert.Equal(1000m, await first.BalanceAsync(a.Id));
        Assert.Equal(1000m, await second.BalanceAsync(b.Id));
    }

    private async Task AssertFailedAsync(HttpResponseMessage response, Guid sourceId, string errorCode)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(errorCode, problem.GetProperty("errorCode").GetString());

        await using var dbContext = factory.CreateDbContext();
        var failed = await dbContext.Transactions.SingleAsync(t => t.SourceAccountId == sourceId && t.Type == TransactionType.Transfer);
        Assert.Equal(TransactionStatus.Failed, failed.Status);
        Assert.Equal(errorCode, failed.FailureReason);
    }
}
