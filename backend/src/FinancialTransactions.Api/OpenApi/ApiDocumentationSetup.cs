using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace FinancialTransactions.Api.OpenApi;

public static class ApiDocumentationSetup
{
    private const string Title = "Financial Transactions API";
    private const string BearerScheme = "Bearer";

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

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "POST /api/v1/auth/login ile alınan token",
            };
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = BearerScheme } }] = [],
            });

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
