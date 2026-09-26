namespace Iccu.Presentation.Authentication;

using Iccu.Domain.Common;
using Microsoft.AspNetCore.Http;
using Iccu.Application.Authentication;
using Iccu.Presentation.Common.Results;

internal static class SessionResults
{
    public static IResult ToSessionResponse(this Result<AuthSession> result, HttpResponse response)
    {
        if (result.IsFailure)
        {
            RefreshTokenCookie.Clear(response);

            return ApiResults.Problem(result);
        }

        RefreshTokenCookie.Write(response, result.Data);

        return Results.Ok(Result.Success(AccessTokenResponse.From(result.Data)));
    }
}
