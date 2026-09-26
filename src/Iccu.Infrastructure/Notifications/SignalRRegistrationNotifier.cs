namespace Iccu.Infrastructure.Notifications;

using Iccu.Presentation.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Iccu.Application.Abstractions.Notifications;

internal sealed class SignalRRegistrationNotifier(
    IHubContext<RegistrationHub> hubContext,
    ILogger<SignalRRegistrationNotifier> logger) : IRegistrationNotifier
{
    public async Task NotifySubmittedAsync(RegistrationSubmittedNotice notice, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.All.SendAsync(
                RegistrationHub.RegistrationSubmittedMethod,
                notice,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Could not notify reception about registration {Code}", notice.Code);
        }
    }
}
