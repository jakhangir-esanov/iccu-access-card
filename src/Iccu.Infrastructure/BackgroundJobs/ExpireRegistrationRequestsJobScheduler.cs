namespace Iccu.Infrastructure.BackgroundJobs;

using Hangfire;
using Microsoft.Extensions.Hosting;

internal sealed class ExpireRegistrationRequestsJobScheduler(
    IRecurringJobManager recurringJobManager) : IHostedService
{
    private const string JobId = "iccu-expire-registration-requests";
    private const string EveryFifteenMinutes = "*/15 * * * *";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobManager.AddOrUpdate<ExpireRegistrationRequestsJob>(
            JobId,
            job => job.ExecuteAsync(CancellationToken.None),
            EveryFifteenMinutes);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
