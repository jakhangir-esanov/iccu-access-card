namespace Iccu.Presentation.Readers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Application.Common.Paging;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Presentation.Readers.Requests;
using Iccu.Application.Readers.GetReaders;
using Iccu.Application.Abstractions.Authentication;

internal sealed class GetReaders : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("readers", async (
            [FromServices] ISender sender,
            [AsParameters] GetReadersRequest request) =>
        {
            var query = new GetReadersQuery(
                new PagingRequest<ReaderListItemResponse>(
                    request.First ?? 0,
                    request.Rows ?? 10,
                    request.SortField ?? "card_number",
                    request.SortOrder ?? -1),
                request.Search,
                request.Category,
                request.Source,
                request.Status,
                request.Gender,
                request.Citizenship,
                request.RegisteredFrom,
                request.RegisteredTo);

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.User)
        .Produces<PagedList<ReaderListItemResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Readers");
    }
}
