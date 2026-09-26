namespace Iccu.Presentation.Readers;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Presentation.Readers.Requests;
using Iccu.Application.Readers.UpdateReader;
using Iccu.Application.Abstractions.Authentication;

internal sealed class UpdateReader : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("readers/{id:guid}", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] UpdateReaderRequest request) =>
        {
            var command = new UpdateReaderCommand(
                id,
                new PersonDetails(
                    request.Category,
                    request.LastName,
                    request.FirstName,
                    request.MiddleName,
                    request.BirthDate,
                    request.Phone,
                    request.DocumentType,
                    request.DocumentNumber),
                request.PhotoFileId);

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
        .WithTags("Readers");
    }
}
