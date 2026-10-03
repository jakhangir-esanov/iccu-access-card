namespace Iccu.Presentation.Koha;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Koha.GetKohaReaderPhoto;
using Iccu.Application.Abstractions.Authentication;

internal sealed class GetKohaReaderPhoto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("koha/readers/{cardNumber:int}/photo", async (
            [FromServices] ISender sender,
            int cardNumber) =>
        {
            var result = await sender.Send(new GetKohaReaderPhotoQuery(cardNumber));

            if (result.IsFailure)
            {
                return ApiResults.Problem(result);
            }

            return Results.File(result.Data.Content, result.Data.ContentType);
        })
        .RequireAuthorization(Policies.Koha)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Koha");
    }
}
