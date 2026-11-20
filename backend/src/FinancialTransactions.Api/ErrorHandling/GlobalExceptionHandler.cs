using FinancialTransactions.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace FinancialTransactions.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, errorCode) = exception switch
        {
            NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.ErrorCode),
            ConflictException conflict => (StatusCodes.Status409Conflict, conflict.ErrorCode),
            BusinessRuleException rule => (StatusCodes.Status422UnprocessableEntity, rule.ErrorCode),
            _ => (StatusCodes.Status500InternalServerError, ErrorCodes.InternalError),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = statusCode, Extensions = { [ProblemDetailsSetup.ErrorCodeKey] = errorCode } },
        });
    }
}
