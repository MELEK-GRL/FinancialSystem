using FinancialTransactions.Application.Common.Options;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddOptions<TransactionOptions>()
            .BindConfiguration(TransactionOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
