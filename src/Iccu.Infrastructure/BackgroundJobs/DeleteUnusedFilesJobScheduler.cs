namespace Iccu.Infrastructure.BackgroundJobs;

using Hangfire;
using Microsoft.Extensions.Hosting;

internal sealed class DeleteUnusedFilesJobScheduler(
    IRecurringJobManager recurringJobManager) : IHostedService
{
    private const string JobId = "iccu-delete-unused-files";
    private const string Hourly = "0 * * * *";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobManager.AddOrUpdate<DeleteUnusedFilesJob>(
            JobId,
            job => job.ExecuteAsync(CancellationToken.None),
            Hourly);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
