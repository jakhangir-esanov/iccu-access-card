namespace Iccu.Presentation.Readers;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Application.Readers.GetReader;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Abstractions.Authentication;

internal sealed class GetReader : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("readers/{id:guid}", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new GetReaderQuery(id));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.User)
        .Produces<Result<ReaderResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Readers");
    }
}
