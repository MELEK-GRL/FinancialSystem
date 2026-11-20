using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Api.Middleware;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Audit;
using FinancialTransactions.Domain.Users;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FinancialTransactions.IntegrationTests.Accounts;

public class AccountStatusTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Admin_DeactivatesAccount_AndAuditIsWrittenWithReasonAndCorrelationId()
    {
        var accountId = await OpenAccountAsync();
        var admin = await factory.CreateAdminClientAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/accounts/{accountId}/status")
        {
            Content = JsonContent.Create(new { status = "Passive", reason = "Şüpheli işlem incelemesi" }),
        };
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, "audit-corr-1");
        var response = await admin.SendAsync(request);
        var account = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Passive", account.GetProperty("status").GetString());

        await using var dbContext = factory.CreateDbContext();
        var audit = await dbContext.AuditLogs.SingleAsync(log => log.AccountId == accountId);
        Assert.Equal(AuditAction.AccountDeactivated, audit.Action);
        Assert.Equal(AccountStatus.Active, audit.OldStatus);
        Assert.Equal("Şüpheli işlem incelemesi", audit.Reason);
        Assert.Equal("audit-corr-1", audit.CorrelationId);
        Assert.True(await dbContext.Users.AnyAsync(user => user.Id == audit.PerformedBy && user.Role == UserRole.Admin));
    }

    [Fact]
    public async Task SameStatus_DoesNotWriteAudit()
    {
        var accountId = await OpenAccountAsync();
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PatchAsJsonAsync($"/api/v1/accounts/{accountId}/status", new { status = "Active", reason = "Zaten aktif" });

        await using var dbContext = factory.CreateDbContext();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(await dbContext.AuditLogs.AnyAsync(log => log.AccountId == accountId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("kısa")]
    public async Task MissingOrShortReason_Returns400(string? reason)
    {
        var accountId = await OpenAccountAsync();
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PatchAsJsonAsync($"/api/v1/accounts/{accountId}/status", new { status = "Passive", reason });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.GetProperty("errors").TryGetProperty("reason", out _));
    }

    [Fact]
    public async Task Customer_CannotChangeAccountStatus()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = (await (await customer.Client.PostAsync("/api/v1/accounts", null)).Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var response = await customer.Client.PatchAsJsonAsync($"/api/v1/accounts/{accountId}/status", new { status = "Passive", reason = "Kendi hesabım" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WhenAuditCannotBeWritten_StatusChangeIsRolledBack()
    {
        var accountId = await OpenAccountAsync();
        var adminNotInDatabase = User.CreateAdmin(TestData.UniqueEmail(), "hash");
        var token = factory.Services.GetRequiredService<IJwtTokenService>().Create(adminNotInDatabase).Token;
        var admin = factory.CreateAuthorizedClient(token);

        var response = await admin.PatchAsJsonAsync($"/api/v1/accounts/{accountId}/status", new { status = "Passive", reason = "Şüpheli işlem" });

        await using var dbContext = factory.CreateDbContext();
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(AccountStatus.Active, (await dbContext.Accounts.SingleAsync(a => a.Id == accountId)).Status);
        Assert.False(await dbContext.AuditLogs.AnyAsync(log => log.AccountId == accountId));
    }

    [Fact]
    public async Task AuditLogs_CannotBeUpdatedOrDeleted()
    {
        var accountId = await OpenAccountAsync();
        var admin = await factory.CreateAdminClientAsync();
        await admin.PatchAsJsonAsync($"/api/v1/accounts/{accountId}/status", new { status = "Passive", reason = "Şüpheli işlem" });

        await using var dbContext = factory.CreateDbContext();
        var update = await Assert.ThrowsAsync<PostgresException>(() => dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE audit_logs SET reason = 'değiştirildi' WHERE account_id = {accountId}"));
        var delete = await Assert.ThrowsAsync<PostgresException>(() => dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM audit_logs WHERE account_id = {accountId}"));

        Assert.Contains("append-only", update.MessageText);
        Assert.Contains("append-only", delete.MessageText);
        Assert.Equal("Şüpheli işlem", (await dbContext.AuditLogs.SingleAsync(log => log.AccountId == accountId)).Reason);
    }

    private async Task<Guid> OpenAccountAsync()
    {
        var customer = await factory.RegisterCustomerAsync();
        var response = await customer.Client.PostAsync("/api/v1/accounts", null);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }
}
