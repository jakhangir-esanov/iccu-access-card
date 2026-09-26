namespace Iccu.Presentation.Realtime;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Iccu.Application.Abstractions.Authentication;

[Authorize(Policy = Policies.User)]
public sealed class RegistrationHub : Hub
{
    public const string Route = "hubs/registrations";

    public const string RegistrationSubmittedMethod = "RegistrationSubmitted";
}
