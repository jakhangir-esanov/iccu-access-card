namespace Iccu.Presentation.StoredFiles;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Abstractions.Authentication;
using Iccu.Application.StoredFiles.GetFileContent;

internal sealed class GetFileContent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{id:guid}/content", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new GetFileContentQuery(id));

            if (result.IsFailure)
            {
                return ApiResults.Problem(result);
            }

            return Results.File(
                result.Data.Content,
                result.Data.ContentType,
                enableRangeProcessing: true);
        })
        .RequireAuthorization(Policies.User)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Files");
    }
}
