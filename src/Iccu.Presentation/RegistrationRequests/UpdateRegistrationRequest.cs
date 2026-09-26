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
using Iccu.Presentation.RegistrationRequests.Requests;
using Iccu.Application.RegistrationRequests.UpdateRegistrationRequest;

internal sealed class UpdateRegistrationRequest : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("registration-requests/{id:guid}", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] UpdateRegistrationRequestRequest request) =>
        {
            var command = new UpdateRegistrationRequestCommand(
                id,
                new PersonDetails(
                    request.Category,
                    request.LastName,
                    request.FirstName,
                    request.MiddleName,
                    request.BirthDate,
                    request.Phone,
                    request.DocumentType,
                    request.DocumentNumber));

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.User)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Registration requests");
    }
}
