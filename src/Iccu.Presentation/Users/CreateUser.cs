namespace Iccu.Presentation.Users;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Users.Requests;
using Iccu.Application.Users.CreateUser;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Abstractions.Authentication;

internal sealed class CreateUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users", async (
            [FromServices] ISender sender,
            [FromBody] CreateUserRequest request) =>
        {
            var command = new CreateUserCommand(request.Username, request.FullName, request.Role, request.Password);

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Admin)
        .Produces<Result<Guid>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Users");
    }
}
