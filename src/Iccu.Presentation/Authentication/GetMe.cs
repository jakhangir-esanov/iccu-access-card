namespace Iccu.Presentation.Authentication;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Application.Authentication;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Authentication.GetMe;
using Iccu.Application.Abstractions.Authentication;

internal sealed class GetMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/me", async (
            [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetMeQuery());

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.User)
        .Produces<Result<UserProfileResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Authentication");
    }
}
