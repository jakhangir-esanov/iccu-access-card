namespace Iccu.Presentation.StoredFiles;

using MediatR;
using Iccu.Domain.Common;
using Iccu.Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.StoredFiles.UploadFile;

internal sealed class UploadPublicFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("public/files", async (
            [FromServices] ISender sender,
            IFormFile file) =>
        {
            await using var content = file.OpenReadStream();

            var command = new UploadFileCommand(file.FileName, file.Length, content);

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .AllowAnonymous()
        .DisableAntiforgery()
        .RequireRateLimiting(RateLimitPolicies.PublicUpload)
        .Produces<Result<Guid>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Public registration");
    }
}
