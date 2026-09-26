namespace Iccu.Presentation.Authentication;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Abstractions.Authentication;
using Iccu.Presentation.Authentication.Requests;
using Iccu.Application.Authentication.ChangeMyPassword;

internal sealed class ChangeMyPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/change-password", async (
            [FromServices] ISender sender,
            [FromBody] ChangeMyPasswordRequest request,
            HttpResponse response) =>
        {
            var command = new ChangeMyPasswordCommand(request.CurrentPassword, request.NewPassword);

            var result = await sender.Send(command);

            if (result.IsSuccess)
            {
                RefreshTokenCookie.Clear(response);
            }

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.User)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Authentication");
    }
}
