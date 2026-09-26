namespace Iccu.Presentation.Users;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Application.Common.Paging;
using Iccu.Application.Users.GetUsers;
using Iccu.Presentation.Users.Requests;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Abstractions.Authentication;

internal sealed class GetUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async (
            [FromServices] ISender sender,
            [AsParameters] GetUsersRequest request) =>
        {
            var query = new GetUsersQuery(
                new PagingRequest<UserResponse>(
                    request.First ?? 0,
                    request.Rows ?? 10,
                    request.SortField ?? "full_name",
                    request.SortOrder ?? 1),
                request.Search,
                request.Role,
                request.IsActive);

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.Admin)
        .Produces<PagedList<UserResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Users");
    }
}
