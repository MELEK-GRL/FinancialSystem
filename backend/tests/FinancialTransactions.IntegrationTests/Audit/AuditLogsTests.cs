using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Api.Routing;
using FinancialTransactions.IntegrationTests.Infrastructure;

namespace FinancialTransactions.IntegrationTests.Audit;

public class AuditLogsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/v1/audit-logs";

    [Fact]
    public async Task Admin_ListsAuditLogsOfAnAccount_NewestFirst()
    {
        var admin = await factory.CreateAdminClientAsync();
        var (accountId, accountNo) = await OpenAccountAsync();
        await ChangeStatusAsync(admin, accountId, "Passive", "Şüpheli işlem");
        await ChangeStatusAsync(admin, accountId, "Active", "İnceleme tamamlandı");

        var page = await admin.GetFromJsonAsync<JsonElement>($"{Url}?accountId={accountId}");
        var items = page.GetProperty("items");

        Assert.Equal(2, page.GetProperty("totalCount").GetInt32());
        Assert.Equal("AccountActivated", items[0].GetProperty("action").GetString());
        Assert.Equal("İnceleme tamamlandı", items[0].GetProperty("reason").GetString());
        Assert.Equal("AccountDeactivated", items[1].GetProperty("action").GetString());
        Assert.Equal(accountNo, items[0].GetProperty("accountNo").GetString());
        Assert.EndsWith("@example.com", items[0].GetProperty("performedByEmail").GetString());
    }

    [Fact]
    public async Task Admin_FiltersByActionAndDate_WithPaging()
    {
        var admin = await factory.CreateAdminClientAsync();
        var (accountId, _) = await OpenAccountAsync();
        await ChangeStatusAsync(admin, accountId, "Passive", "Şüpheli işlem");
        await ChangeStatusAsync(admin, accountId, "Active", "İnceleme tamamlandı");

        var deactivations = await admin.GetFromJsonAsync<JsonElement>($"{Url}?accountId={accountId}&action=AccountDeactivated");
        var future = await admin.GetFromJsonAsync<JsonElement>($"{Url}?accountId={accountId}&from={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(1).ToString("O"))}");
        var firstPage = await admin.GetFromJsonAsync<JsonElement>($"{Url}?accountId={accountId}&page=1&size=1");

        Assert.Equal(1, deactivations.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, future.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, firstPage.GetProperty("items").GetArrayLength());
        Assert.Equal(2, firstPage.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task FromAfterTo_Returns400()
    {
        var admin = await factory.CreateAdminClientAsync();
        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"));
        var to = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-1).ToString("O"));

        var response = await admin.GetAsync($"{Url}?from={from}&to={to}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Customer_CannotReadAuditLogs()
    {
        var customer = await factory.RegisterCustomerAsync();

        var response = await customer.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("AuditLogs", "audit-logs")]
    [InlineData("Accounts", "accounts")]
    [InlineData("Auth", "auth")]
    public void ControllerNames_BecomeKebabCaseRoutes(string controller, string expected)
    {
        Assert.Equal(expected, new KebabCaseParameterTransformer().TransformOutbound(controller));
    }

    private async Task<(Guid Id, string AccountNo)> OpenAccountAsync()
    {
        var customer = await factory.RegisterCustomerAsync();
        var account = await (await customer.Client.PostAsync("/api/v1/accounts", null)).Content.ReadFromJsonAsync<JsonElement>();
        return (account.GetProperty("id").GetGuid(), account.GetProperty("accountNo").GetString()!);
    }

    private static async Task ChangeStatusAsync(HttpClient admin, Guid accountId, string status, string reason)
    {
        var response = await admin.PatchAsJsonAsync($"/api/v1/accounts/{accountId}/status", new { status, reason });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
