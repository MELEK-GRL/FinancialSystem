using System.Text.Json.Serialization;
using FinancialTransactions.Api.ErrorHandling;
using FinancialTransactions.Api.Health;
using FinancialTransactions.Api.Middleware;
using FinancialTransactions.Api.OpenApi;
using FinancialTransactions.Application;
using FinancialTransactions.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
});

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

builder.Services.AddLocalization();
builder.Services.AddApiProblemDetails();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddApiDocumentation();
builder.Services.AddApiHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRequestLocalization(options => options
    .SetDefaultCulture("tr")
    .AddSupportedCultures("tr", "en")
    .AddSupportedUICultures("tr", "en"));
app.UseHttpLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapApiHealthChecks();
app.MapApiDocumentation();
app.MapControllers();

app.Run();

public partial class Program;
