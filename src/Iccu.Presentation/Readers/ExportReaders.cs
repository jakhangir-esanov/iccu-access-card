namespace Iccu.Presentation.Readers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Presentation.Readers.Requests;
using Iccu.Application.Abstractions.Authentication;
using Iccu.Application.Readers.ExportReaders;

internal sealed class ExportReaders : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("readers/export", async (
            [FromServices] ISender sender,
            [AsParameters] ExportReadersRequest request) =>
        {
            var query = new ExportReadersQuery(
                request.Search,
                request.Category,
                request.Source,
                request.Status,
                request.RegisteredFrom,
                request.RegisteredTo);

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return ApiResults.Problem(result);
            }

            return Results.File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
        })
        .RequireAuthorization(Policies.Admin)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Readers");
    }
}
