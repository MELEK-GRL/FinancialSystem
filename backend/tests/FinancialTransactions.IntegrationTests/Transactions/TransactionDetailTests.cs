using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Transactions;

public class TransactionDetailTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task LocationHeader_ReturnsTheCreatedTransaction()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();

        var created = await customer.Client.PostAsJsonAsync("/api/v1/transactions/deposit", new { accountId, amount = 75m, description = "Maaş" });
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>();
        var detail = await customer.Client.GetFromJsonAsync<JsonElement>(created.Headers.Location);

        Assert.Equal(createdBody.GetProperty("id").GetGuid(), detail.GetProperty("id").GetGuid());
        Assert.Equal("Deposit", detail.GetProperty("type").GetString());
        Assert.Equal("Success", detail.GetProperty("status").GetString());
        Assert.Equal(75m, detail.GetProperty("amount").GetDecimal());
        Assert.Equal("Maaş", detail.GetProperty("description").GetString());
        Assert.NotEqual(JsonValueKind.Null, detail.GetProperty("completedAt").ValueKind);
    }

    [Fact]
    public async Task TransferIsVisibleToSenderReceiverAndAdmin_ButNotToOthers()
    {
        var sender = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var stranger = await factory.RegisterCustomerAsync();
        var sourceId = await sender.OpenAccountAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await sender.DepositAsync(sourceId, 100m);
        var referenceNo = await TransferAsync(sender, sourceId, target.AccountNo, 30m);
        var admin = await factory.CreateAdminClientAsync();
        var url = $"/api/v1/transactions/{referenceNo}";

        Assert.Equal(HttpStatusCode.OK, (await sender.Client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await receiver.Client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync(url)).StatusCode);
        await AssertNotFoundAsync(await stranger.Client.GetAsync(url));
    }

    [Fact]
    public async Task FailedTransaction_IsVisibleWithFailureReason()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        var withdraw = await customer.Client.PostAsJsonAsync("/api/v1/transactions/withdraw", new { accountId, amount = 10m });
        var referenceNo = await FindReferenceNoAsync(customer, withdraw);

        var detail = await customer.Client.GetFromJsonAsync<JsonElement>($"/api/v1/transactions/{referenceNo}");

        Assert.Equal("Failed", detail.GetProperty("status").GetString());
        Assert.Equal(ErrorCodes.InsufficientBalance, detail.GetProperty("failureReason").GetString());
    }

    [Fact]
    public async Task ReferenceNo_IsCaseInsensitive()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        var created = await customer.Client.PostAsJsonAsync("/api/v1/transactions/deposit", new { accountId, amount = 5m });
        var referenceNo = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("referenceNo").GetString()!;

        var response = await customer.Client.GetAsync($"/api/v1/transactions/{referenceNo.ToLowerInvariant()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnknownReference_Returns404()
    {
        var customer = await factory.RegisterCustomerAsync();

        await AssertNotFoundAsync(await customer.Client.GetAsync("/api/v1/transactions/TX2099010100000001"));
    }

    [Theory]
    [InlineData("deposit")]
    [InlineData("withdraw")]
    [InlineData("transfer")]
    public async Task Admin_CannotCreateTransactions(string operation)
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync($"/api/v1/transactions/{operation}", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/transactions/TX2099010100000001");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<string> TransferAsync(TestCustomer sender, Guid sourceId, string targetAccountNo, decimal amount)
    {
        var response = await sender.Client.PostAsJsonAsync(
            "/api/v1/transactions/transfer", new { sourceAccountId = sourceId, targetAccountNo, amount });
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("referenceNo").GetString()!;
    }

    private async Task<string> FindReferenceNoAsync(TestCustomer customer, HttpResponseMessage failedResponse)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, failedResponse.StatusCode);
        await using var dbContext = factory.CreateDbContext();
        var failed = await dbContext.Transactions.SingleAsync(t => t.InitiatedByCustomerId == customer.CustomerId);
        return failed.ReferenceNo;
    }

    private static async Task AssertNotFoundAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ErrorCodes.TransactionNotFound, problem.GetProperty("errorCode").GetString());
    }
}
