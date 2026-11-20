using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.IntegrationTests.Validation;

public class ValidationPipelineTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client = factory
        .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(SampleCommandController).Assembly);
            services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<SampleCommandController>());
            services.AddValidatorsFromAssemblyContaining<SampleCommandController>(includeInternalTypes: true);
        }))
        .CreateClient();

    [Fact]
    public async Task InvalidCommand_Returns400WithFieldErrors()
    {
        var response = await PostAsync(new SampleCommand("", -1), "tr");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ErrorCodes.ValidationError, problem.GetProperty("errorCode").GetString());
        Assert.Equal("Gönderilen bilgiler geçersiz", problem.GetProperty("title").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty("name", out _));
        Assert.True(problem.GetProperty("errors").TryGetProperty("amount", out _));
    }

    [Fact]
    public async Task FieldErrors_FollowRequestLanguage()
    {
        var turkish = await ReadAmountErrorAsync("tr");
        var english = await ReadAmountErrorAsync("en");

        Assert.NotEqual(turkish, english);
        Assert.Contains("greater than", english);
    }

    [Fact]
    public async Task ValidCommand_ReachesHandler()
    {
        var response = await PostAsync(new SampleCommand("Ayşe", 10), "tr");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Ayşe", await response.Content.ReadAsStringAsync());
    }

    private async Task<string> ReadAmountErrorAsync(string language)
    {
        var response = await PostAsync(new SampleCommand("Ayşe", -1), language);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        return problem.GetProperty("errors").GetProperty("amount")[0].GetString()!;
    }

    private Task<HttpResponseMessage> PostAsync(SampleCommand command, string language)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/test-commands") { Content = JsonContent.Create(command) };
        request.Headers.AcceptLanguage.ParseAdd(language);
        return client.SendAsync(request);
    }
}

public sealed record SampleCommand(string Name, decimal Amount) : IRequest<string>;

internal sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
{
    public SampleCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty();
        RuleFor(command => command.Amount).GreaterThan(0);
    }
}

internal sealed class SampleCommandHandler : IRequestHandler<SampleCommand, string>
{
    public Task<string> Handle(SampleCommand request, CancellationToken cancellationToken) => Task.FromResult(request.Name);
}

[AllowAnonymous]
[ApiController]
[Route("test-commands")]
public class SampleCommandController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<string> Send(SampleCommand command, CancellationToken cancellationToken) =>
        await sender.Send(command, cancellationToken);
}
