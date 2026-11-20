using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace FinancialTransactions.Api.ErrorHandling;

public static class InvalidModelStateResponse
{
    private const string BodyField = "body";

    public static IActionResult Create(ActionContext context)
    {
        var parameterNames = context.ActionDescriptor.Parameters.Select(parameter => parameter.Name).ToHashSet();
        var message = Messages.Get(Messages.InvalidValue);

        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .Select(entry => ToFieldName(entry.Key, parameterNames))
            .Distinct()
            .ToDictionary(field => field, _ => new[] { message });

        var problem = context.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>()
            .CreateProblemDetails(context.HttpContext, StatusCodes.Status400BadRequest);
        problem.Extensions[ProblemDetailsSetup.ErrorCodeKey] = ErrorCodes.ValidationError;
        problem.Extensions["errors"] = errors;

        return new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" },
        };
    }

    private static string ToFieldName(string key, HashSet<string> parameterNames)
    {
        var name = key.TrimStart('$', '.');

        if (name.Length == 0 || parameterNames.Contains(name))
        {
            return BodyField;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
