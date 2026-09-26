namespace Iccu.Infrastructure.BackgroundJobs;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.Extensions.Logging;
using Iccu.Application.RegistrationRequests.ExpireRegistrationRequests;

public sealed class ExpireRegistrationRequestsJob(
    ISender sender,
    ILogger<ExpireRegistrationRequestsJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Result<int> result = await sender.Send(new ExpireRegistrationRequestsCommand(), cancellationToken);

        if (result.IsSuccess && result.Data > 0)
        {
            logger.LogInformation("Expired {Count} stale registration requests", result.Data);
        }
    }
}
