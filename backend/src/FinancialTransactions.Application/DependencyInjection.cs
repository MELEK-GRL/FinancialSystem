using FinancialTransactions.Application.Common.Behaviors;
using FinancialTransactions.Application.Common.Options;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddOptions<TransactionOptions>()
            .BindConfiguration(TransactionOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
