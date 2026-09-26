namespace Iccu.Infrastructure.BackgroundJobs;

using MediatR;
using Iccu.Domain.Common;
using Microsoft.Extensions.Logging;
using Iccu.Application.StoredFiles.DeleteUnusedFiles;

public sealed class DeleteUnusedFilesJob(
    ISender sender,
    ILogger<DeleteUnusedFilesJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Result<int> result = await sender.Send(new DeleteUnusedFilesCommand(), cancellationToken);

        if (result.IsSuccess && result.Data > 0)
        {
            logger.LogInformation("Deleted {Count} unused files", result.Data);
        }
    }
}
