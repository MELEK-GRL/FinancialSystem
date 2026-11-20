using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace FinancialTransactions.IntegrationTests.Configuration;

public class ConfigurationValidationTests
{
    [Fact]
    public void Application_StartsWithValidConfiguration()
    {
        using var factory = new WebApplicationFactory<Program>();

        var exception = Record.Exception(() => factory.CreateClient());

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("Jwt:Secret", "too-short")]
    [InlineData("Database:ConnectionString", "")]
    [InlineData("Transaction:MaxAmount", "0")]
    public void Application_DoesNotStart_WhenSettingIsInvalid(string key, string value)
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?> { [key] = value })));

        var exception = Assert.ThrowsAny<OptionsValidationException>(() => factory.CreateClient());

        Assert.Contains(key.Split(':')[0], exception.Message);
    }
}
