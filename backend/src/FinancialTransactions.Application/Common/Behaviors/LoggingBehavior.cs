using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinancialTransactions.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            logger.LogInformation("{RequestName} handled in {ElapsedMilliseconds} ms", requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogWarning("{RequestName} failed after {ElapsedMilliseconds} ms with {ExceptionType}", requestName, stopwatch.ElapsedMilliseconds, exception.GetType().Name);
            throw;
        }
    }
}
