using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Transactions;

public class IdempotencyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task SameKeyAndSameRequest_ReturnsFirstResult_AndMovesMoneyOnce()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        var key = NewKey();

        var first = await PostAsync(customer, "deposit", new { accountId, amount = 100m }, key);
        var second = await PostAsync(customer, "deposit", new { accountId, amount = 100m }, key);
        var firstBody = await first.Content.ReadFromJsonAsync<JsonElement>();
        var secondBody = await second.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(firstBody.GetProperty("referenceNo").GetString(), secondBody.GetProperty("referenceNo").GetString());
        Assert.Equal(100m, await customer.BalanceAsync(accountId));
        Assert.Equal(1, await CountAsync(customer.CustomerId, key));
    }

    [Fact]
    public async Task SameKeyWithDifferentRequest_Returns409()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        var key = NewKey();
        await PostAsync(customer, "deposit", new { accountId, amount = 100m }, key);

        var response = await PostAsync(customer, "deposit", new { accountId, amount = 200m }, key);

        await AssertProblemAsync(response, HttpStatusCode.Conflict, ErrorCodes.IdempotencyConflict);
        Assert.Equal(100m, await customer.BalanceAsync(accountId));
    }

    [Fact]
    public async Task SameKeyFromDifferentCustomers_DoesNotCollide()
    {
        var first = await factory.RegisterCustomerAsync();
        var second = await factory.RegisterCustomerAsync();
        var firstAccount = await first.OpenAccountAsync();
        var secondAccount = await second.OpenAccountAsync();
        var key = NewKey();

        var firstResponse = await PostAsync(first, "deposit", new { accountId = firstAccount, amount = 10m }, key);
        var secondResponse = await PostAsync(second, "deposit", new { accountId = secondAccount, amount = 20m }, key);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.Equal(20m, await second.BalanceAsync(secondAccount));
    }

    [Fact]
    public async Task FailedRequest_IsReplayedAsTheSameFailure()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        var key = NewKey();

        var first = await PostAsync(customer, "withdraw", new { accountId, amount = 50m }, key);
        await customer.DepositAsync(accountId, 100m);
        var retry = await PostAsync(customer, "withdraw", new { accountId, amount = 50m }, key);

        await AssertProblemAsync(first, HttpStatusCode.UnprocessableEntity, ErrorCodes.InsufficientBalance);
        await AssertProblemAsync(retry, HttpStatusCode.UnprocessableEntity, ErrorCodes.InsufficientBalance);
        Assert.Equal(100m, await customer.BalanceAsync(accountId));
        Assert.Equal(1, await CountAsync(customer.CustomerId, key));
    }

    [Fact]
    public async Task ParallelTransfersWithSameKey_CreateSingleTransaction()
    {
        var sender = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var sourceId = await sender.OpenAccountAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await sender.DepositAsync(sourceId, 1000m);
        var key = NewKey();
        var body = new { sourceAccountId = sourceId, targetAccountNo = target.AccountNo, amount = 100m };

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => PostAsync(sender, "transfer", body, key)));

        Assert.All(responses, response => Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }));
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Equal(900m, await sender.BalanceAsync(sourceId));
        Assert.Equal(100m, await receiver.BalanceAsync(target.Id));
        Assert.Equal(1, await CountAsync(sender.CustomerId, key));
    }

    [Fact]
    public async Task WithoutKey_EveryRequestCreatesANewTransaction()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        await customer.DepositAsync(accountId, 10m);
        await customer.DepositAsync(accountId, 10m);

        Assert.Equal(20m, await customer.BalanceAsync(accountId));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("has spaces in key")]
    public async Task InvalidKey_Returns400(string key)
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var response = await PostAsync(customer, "deposit", new { accountId, amount = 10m }, key);

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    private static string NewKey() => Guid.NewGuid().ToString();

    private static Task<HttpResponseMessage> PostAsync(TestCustomer customer, string operation, object body, string key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/transactions/{operation}") { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", key);
        return customer.Client.SendAsync(request);
    }

    private async Task<int> CountAsync(Guid customerId, string key)
    {
        await using var dbContext = factory.CreateDbContext();
        return await dbContext.Transactions.CountAsync(t => t.InitiatedByCustomerId == customerId && t.IdempotencyKey == key);
    }

    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode status, string errorCode)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(errorCode, problem.GetProperty("errorCode").GetString());
    }
}
