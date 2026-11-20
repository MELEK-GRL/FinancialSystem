using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, errorCode) = exception switch
        {
            RequestValidationException => (StatusCodes.Status400BadRequest, ErrorCodes.ValidationError),
            NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.ErrorCode),
            ConflictException conflict => (StatusCodes.Status409Conflict, conflict.ErrorCode),
            BusinessRuleException rule => (StatusCodes.Status422UnprocessableEntity, rule.ErrorCode),
            _ => (StatusCodes.Status500InternalServerError, ErrorCodes.InternalError),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        var problem = new ProblemDetails { Status = statusCode };
        problem.Extensions[ProblemDetailsSetup.ErrorCodeKey] = errorCode;

        if (exception is RequestValidationException validation)
        {
            problem.Extensions["errors"] = validation.Errors;
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }
}
