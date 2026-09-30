namespace Iccu.Presentation.RegistrationRequests;

using MediatR;
using Iccu.Domain.Common;
using Iccu.Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Results;
using Iccu.Presentation.Common.Endpoints;
using Iccu.Presentation.RegistrationRequests.Requests;
using Iccu.Application.RegistrationRequests.SubmitRegistration;

internal sealed class SubmitRegistration : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("public/registrations", async (
            [FromServices] ISender sender,
            [FromBody] SubmitRegistrationRequest request) =>
        {
            var command = new SubmitRegistrationCommand(
                new PersonDetails(
                    request.Category,
                    request.LastName,
                    request.FirstName,
                    request.MiddleName,
                    request.BirthDate,
                    request.Gender,
                    request.Citizenship,
                    request.Phone),
                request.PhotoFileId,
                request.ConsentGiven);

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitPolicies.PublicRegistration)
        .Produces<Result<SubmitRegistrationResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Public registration");
    }
}
