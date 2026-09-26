namespace Iccu.Presentation.RegistrationRequests;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Application.Common.Paging;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Abstractions.Authentication;
using Iccu.Presentation.RegistrationRequests.Requests;
using Iccu.Application.RegistrationRequests.GetRegistrationRequests;

internal sealed class GetRegistrationRequests : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("registration-requests", async (
            [FromServices] ISender sender,
            [AsParameters] GetRegistrationRequestsRequest request) =>
        {
            var query = new GetRegistrationRequestsQuery(
                new PagingRequest<RegistrationRequestListItemResponse>(
                    request.First ?? 0,
                    request.Rows ?? 10,
                    request.SortField ?? "submitted_at",
                    request.SortOrder ?? -1),
                request.Status,
                request.Search);

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.User)
        .Produces<PagedList<RegistrationRequestListItemResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Registration requests");
    }
}
