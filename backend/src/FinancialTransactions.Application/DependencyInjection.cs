using FinancialTransactions.Application.Common.Behaviors;
using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Application.Common.Options;
using FinancialTransactions.Application.Transactions;
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

        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member is null ? null : Messages.FieldName(member.Name);
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<TransactionExecutor>();

        return services;
    }
}
