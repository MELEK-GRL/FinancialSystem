using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Domain.Common;
using Microsoft.Extensions.Localization;

namespace FinancialTransactions.Api.ErrorHandling;

public static class ProblemDetailsSetup
{
    public const string ErrorCodeKey = "errorCode";

    private static readonly Dictionary<int, string> ErrorCodesByStatus = new()
    {
        [StatusCodes.Status400BadRequest] = ErrorCodes.ValidationError,
        [StatusCodes.Status401Unauthorized] = ErrorCodes.Unauthorized,
        [StatusCodes.Status403Forbidden] = ErrorCodes.Forbidden,
        [StatusCodes.Status404NotFound] = ErrorCodes.NotFound,
        [StatusCodes.Status429TooManyRequests] = ErrorCodes.TooManyRequests,
    };

    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;
            var status = problem.Status ?? StatusCodes.Status500InternalServerError;

            if (!problem.Extensions.TryGetValue(ErrorCodeKey, out var value) || value is not string errorCode)
            {
                errorCode = ErrorCodesByStatus.GetValueOrDefault(status, ErrorCodes.InternalError);
                problem.Extensions[ErrorCodeKey] = errorCode;
            }

            var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Messages>>();

            problem.Title = localizer[errorCode];
            problem.Detail = null;
            problem.Instance = context.HttpContext.Request.Path;
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        });

        return services;
    }
}
