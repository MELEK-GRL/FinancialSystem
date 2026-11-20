using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace FinancialTransactions.Api.OpenApi;

public static class ApiDocumentationSetup
{
    private const string Title = "Financial Transactions API";

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = Title,
                Version = "v1",
                Description = "Müşteri, hesap, para yatırma, çekme, transfer, işlem geçmişi ve raporlama API'si.",
            };
            return Task.CompletedTask;
        }));

        return services;
    }

    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options => options.WithTitle(Title));

        return app;
    }
}
