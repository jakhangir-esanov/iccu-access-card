namespace Iccu.Presentation.Authentication;

using MediatR;
using Iccu.Domain.Common;
using Iccu.Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Authentication.RefreshSession;

internal sealed class RefreshSession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (
            [FromServices] ISender sender,
            HttpRequest request,
            HttpResponse response) =>
        {
            var command = new RefreshSessionCommand(RefreshTokenCookie.Read(request));

            var result = await sender.Send(command);

            return result.ToSessionResponse(response);
        })
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitPolicies.Login)
        .Produces<Result<AccessTokenResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Authentication");
    }
}
