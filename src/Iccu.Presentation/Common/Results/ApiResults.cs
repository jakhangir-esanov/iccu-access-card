namespace Iccu.Presentation.Common.Results;

using Iccu.Domain.Common;
using Microsoft.AspNetCore.Http;

public static class ApiResults
{
    public const string MessagesExtension = "messages";

    private const string ErrorsExtension = "errors";

    public static IResult ToResponse<T>(this Result<T> result)
    {
        return result.Match(
            value => Results.Ok(result),
            Problem);
    }

    public static IResult ToResponse(this Result result)
    {
        return result.Match(
            () => Results.Ok(result),
            Problem);
    }

    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException();
        }

        Error publicError = GetPublicError(result.Error);

        return Results.Problem(
            title: GetTitle(result.Error),
            detail: publicError.Message,
            type: GetType(result.Error.Type),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: GetExtensions(result.Error, publicError));

        static Error GetPublicError(Error error) =>
            error.Type == ErrorType.Failure ? Error.Unexpected : error;

        static string GetTitle(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Code,
                ErrorType.Problem => error.Code,
                ErrorType.Unauthorized => error.Code,
                ErrorType.Forbidden => error.Code,
                ErrorType.NotFound => error.Code,
                ErrorType.Conflict => error.Code,
                _ => "Server failure"
            };

        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Problem => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

        static Dictionary<string, object?> GetExtensions(Error error, Error publicError)
        {
            var extensions = new Dictionary<string, object?>
            {
                { MessagesExtension, publicError.Messages }
            };

            if (error is ValidationError validationError)
            {
                object[] errorsSerializedByRuntimeType = [.. validationError.Errors];

                extensions.Add(ErrorsExtension, errorsSerializedByRuntimeType);
            }

            return extensions;
        }
    }
}
