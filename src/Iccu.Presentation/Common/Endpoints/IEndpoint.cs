namespace Iccu.Presentation.Common.Endpoints;

using Microsoft.AspNetCore.Routing;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
