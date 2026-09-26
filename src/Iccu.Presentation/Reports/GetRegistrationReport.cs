namespace Iccu.Presentation.Reports;

using MediatR;
using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Presentation.Reports.Requests;
using Iccu.Application.Abstractions.Authentication;
using Iccu.Application.Reports.GetRegistrationReport;

internal sealed class GetRegistrationReport : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("reports/registrations", async (
            [FromServices] ISender sender,
            [AsParameters] GetRegistrationReportRequest request) =>
        {
            var query = new GetRegistrationReportQuery(request.From, request.To, request.GroupBy ?? ReportGrouping.Day);

            var result = await sender.Send(query);

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.User)
        .Produces<Result<RegistrationReportResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Reports");
    }
}
