namespace Iccu.Presentation.RegistrationRequests;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Abstractions.Authentication;
using Iccu.Application.RegistrationRequests.ApproveRegistrationRequest;

internal sealed class ApproveRegistrationRequest : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("registration-requests/{id:guid}/approve", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new ApproveRegistrationRequestCommand(id));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.User)
        .Produces<Result<ApproveRegistrationRequestResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Registration requests");
    }
}
