namespace Iccu.Api.Middleware;

using Npgsql;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Iccu.Presentation.Common.Results;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const string UniqueViolation = "23505";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        var problemDetails = IsUniqueViolation(exception)
            ? new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
                Title = Error.DuplicateKey.Code,
                Detail = Error.DuplicateKey.Message,
                Extensions = { [ApiResults.MessagesExtension] = Error.DuplicateKey.Messages }
            }
            : new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Title = Error.Unexpected.Code,
                Extensions = { [ApiResults.MessagesExtension] = Error.Unexpected.Messages }
            };

        await Results.Problem(problemDetails).ExecuteAsync(httpContext);

        return true;
    }

    private static bool IsUniqueViolation(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgresException && postgresException.SqlState == UniqueViolation)
            {
                return true;
            }
        }

        return false;
    }
}
