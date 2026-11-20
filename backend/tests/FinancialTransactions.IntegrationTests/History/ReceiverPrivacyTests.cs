using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.History;

public class ReceiverPrivacyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task FailedTransfer_IsVisibleToSender_ButHiddenFromReceiver()
    {
        var sender = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var sourceId = await sender.OpenAccountAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await sender.Client.PostAsJsonAsync(
            "/api/v1/transactions/transfer", new { sourceAccountId = sourceId, targetAccountNo = target.AccountNo, amount = 50m });
        var referenceNo = await ReferenceNoOfOnlyTransactionAsync(sourceId);
        var admin = await factory.CreateAdminClientAsync();

        var senderItem = Assert.Single(await GetHistoryAsync(sender.Client, sourceId));
        Assert.Equal("Failed", senderItem.GetProperty("status").GetString());
        Assert.Equal(ErrorCodes.InsufficientBalance, senderItem.GetProperty("failureReason").GetString());

        Assert.Empty(await GetHistoryAsync(receiver.Client, target.Id));
        Assert.Equal(HttpStatusCode.NotFound, (await receiver.Client.GetAsync($"/api/v1/transactions/{referenceNo}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await sender.Client.GetAsync($"/api/v1/transactions/{referenceNo}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/api/v1/transactions/{referenceNo}")).StatusCode);
    }

    [Fact]
    public async Task SuccessfulTransfer_IsStillVisibleToReceiver()
    {
        var sender = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var sourceId = await sender.OpenAccountAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        await sender.DepositAsync(sourceId, 50m);
        await sender.Client.PostAsJsonAsync(
            "/api/v1/transactions/transfer", new { sourceAccountId = sourceId, targetAccountNo = target.AccountNo, amount = 50m });

        Assert.Equal("Success", Assert.Single(await GetHistoryAsync(receiver.Client, target.Id)).GetProperty("status").GetString());
    }

    [Fact]
    public async Task OwnFailedTransactions_AreVisibleOnTheTargetAccountToo()
    {
        var customer = await factory.RegisterCustomerAsync();
        var first = await customer.OpenAccountAsync();
        var second = await customer.OpenAccountWithNumberAsync();
        await customer.Client.PostAsJsonAsync(
            "/api/v1/transactions/transfer", new { sourceAccountId = first, targetAccountNo = second.AccountNo, amount = 10m });
        var admin = await factory.CreateAdminClientAsync();
        await admin.PatchAsJsonAsync($"/api/v1/accounts/{second.Id}/status", new { status = "Passive", reason = "Hesap kapatma talebi" });
        await customer.Client.PostAsJsonAsync("/api/v1/transactions/deposit", new { accountId = second.Id, amount = 10m });

        var items = await GetHistoryAsync(customer.Client, second.Id);

        Assert.Equal(["Deposit/Failed", "Transfer/Failed"], items.Select(item => $"{item.GetProperty("type")}/{item.GetProperty("status")}"));
    }

    private async Task<string> ReferenceNoOfOnlyTransactionAsync(Guid sourceAccountId)
    {
        await using var dbContext = factory.CreateDbContext();
        var transaction = await dbContext.Transactions.SingleAsync(t => t.SourceAccountId == sourceAccountId);
        return transaction.ReferenceNo;
    }

    private static async Task<List<JsonElement>> GetHistoryAsync(HttpClient client, Guid accountId)
    {
        var page = await client.GetFromJsonAsync<JsonElement>($"/api/v1/accounts/{accountId}/transactions");
        return page.GetProperty("items").EnumerateArray().ToList();
    }
}
