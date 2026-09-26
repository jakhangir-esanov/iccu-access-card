namespace Iccu.Presentation.Authentication;

using MediatR;
using Iccu.Domain.Common;
using Iccu.Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Authentication.Login;
using Iccu.Presentation.Authentication.Requests;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            [FromServices] ISender sender,
            [FromBody] LoginRequest request,
            HttpResponse response) =>
        {
            var command = new LoginCommand(request.Username, request.Password);

            var result = await sender.Send(command);

            return result.ToSessionResponse(response);
        })
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitPolicies.Login)
        .Produces<Result<AccessTokenResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Authentication");
    }
}
