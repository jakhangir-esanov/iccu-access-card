namespace Iccu.Presentation.Authentication;

using MediatR;
using Iccu.Domain.Common;
using Iccu.Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Authentication.Logout;

internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (
            [FromServices] ISender sender,
            HttpRequest request,
            HttpResponse response) =>
        {
            var command = new LogoutCommand(RefreshTokenCookie.Read(request));

            var result = await sender.Send(command);

            RefreshTokenCookie.Clear(response);

            return result.ToResponse();
        })
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitPolicies.Login)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Authentication");
    }
}
