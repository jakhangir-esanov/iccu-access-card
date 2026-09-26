namespace Iccu.Presentation.Dashboard;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Application.Abstractions.Authentication;
using Iccu.Application.Dashboard.GetDashboard;

internal sealed class GetDashboard : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dashboard", async (
            [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetDashboardQuery());

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.User)
        .Produces<Result<DashboardResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Dashboard");
    }
}
