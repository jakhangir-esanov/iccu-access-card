namespace Iccu.Presentation.Koha;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Koha.Requests;
using Iccu.Application.Common.Paging;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Koha.GetKohaReaders;
using Iccu.Application.Abstractions.Authentication;

internal sealed class GetKohaReaders : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("koha/readers", async (
            [FromServices] ISender sender,
            [AsParameters] GetKohaReadersRequest request) =>
        {
            var query = new GetKohaReadersQuery(
                new PagingRequest<KohaReaderResponse>(
                    request.First ?? 0,
                    request.Rows ?? 1000,
                    "card_number",
                    1));

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.Koha)
        .Produces<PagedList<KohaReaderResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Koha");
    }
}
