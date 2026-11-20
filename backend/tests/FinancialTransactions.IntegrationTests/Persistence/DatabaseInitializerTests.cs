using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Domain.Users;
using FinancialTransactions.Infrastructure.Persistence.Seeding;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class DatabaseInitializerTests : IClassFixture<ApiFactory>
{
    private const string AdminEmail = "admin@example.com";
    private const string AdminPassword = "Admin12345";

    private readonly ApiFactory database;
    private readonly WebApplicationFactory<Program> factory;

    public DatabaseInitializerTests(ApiFactory database)
    {
        this.database = database;
        factory = database.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:AdminEmail"] = AdminEmail,
                ["Seed:AdminPassword"] = AdminPassword,
                ["Seed:CreateDemoCustomers"] = "true",
            })));
    }

    [Fact]
    public async Task Initialize_CreatesAdminAndDemoCustomers_ThatCanLogIn()
    {
        await InitializeAsync();
        var client = factory.CreateClient();

        var admin = await LoginAsync(client, AdminEmail, AdminPassword);
        var demoCustomer = await LoginAsync(client, DemoCustomers.All[0].Email, DemoCustomers.Password);

        Assert.Equal(AppRoles.Admin, admin.GetProperty("role").GetString());
        Assert.Equal(AppRoles.Customer, demoCustomer.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Initialize_IsIdempotent()
    {
        await InitializeAsync();
        await InitializeAsync();

        await using var dbContext = database.CreateDbContext();
        var demoNationalIds = DemoCustomers.All.Select(customer => customer.NationalId).ToList();

        Assert.Equal(1, await dbContext.Users.CountAsync(user => user.Role == UserRole.Admin));
        Assert.Equal(DemoCustomers.All.Count, await dbContext.Customers.CountAsync(customer => demoNationalIds.Contains(customer.NationalId)));
    }

    private async Task InitializeAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(CancellationToken.None);
    }

    private static async Task<JsonElement> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
