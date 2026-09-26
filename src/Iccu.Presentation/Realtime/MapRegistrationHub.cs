namespace Iccu.Presentation.Realtime;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Iccu.Presentation.Common.Endpoints;

internal sealed class MapRegistrationHub : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapHub<RegistrationHub>(RegistrationHub.Route);
    }
}
