namespace Iccu.Presentation.Koha;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Koha.MarkKohaReaderSynced;
using Iccu.Application.Abstractions.Authentication;

internal sealed class MarkKohaReaderSynced : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("koha/readers/{cardNumber:int}/mark-synced", async (
            [FromServices] ISender sender,
            int cardNumber) =>
        {
            var result = await sender.Send(new MarkKohaReaderSyncedCommand(cardNumber));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Koha)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Koha");
    }
}
