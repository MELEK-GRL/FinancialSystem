using System.Text.Json.Serialization;
using FinancialTransactions.Api.ErrorHandling;
using FinancialTransactions.Api.Health;
using FinancialTransactions.Api.Middleware;
using FinancialTransactions.Api.OpenApi;
using FinancialTransactions.Api.Routing;
using FinancialTransactions.Api.Security;
using FinancialTransactions.Api.Services;
using FinancialTransactions.Application;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Infrastructure;
using FinancialTransactions.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddApiAuthentication();

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = InvalidModelStateResponse.Create);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapApiHealthChecks();
app.MapApiDocumentation();
app.MapControllers();

await app.Services.InitializeDatabaseAsync();

app.Run();

public partial class Program;
