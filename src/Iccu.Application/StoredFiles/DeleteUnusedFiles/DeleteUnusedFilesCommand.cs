namespace Iccu.Application.StoredFiles.DeleteUnusedFiles;

using Iccu.Domain.Common;
using Iccu.Domain.StoredFiles;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Storage;

public sealed record DeleteUnusedFilesCommand : ICommand<int>;

internal sealed class DeleteUnusedFilesCommandHandler(
    IUnitOfWork unitOfWork,
    IFileStore fileStore,
    IStoredFileRepository storedFileRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<DeleteUnusedFilesCommand, int>
{
    private const int BatchSize = 200;

    private static readonly TimeSpan GracePeriod = TimeSpan.FromHours(24);

    public async Task<Result<int>> Handle(DeleteUnusedFilesCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<StoredFile> unusedFiles = await storedFileRepository.GetUnusedAsync(
            dateTimeProvider.UtcNow.Subtract(GracePeriod), BatchSize, cancellationToken);

        if (unusedFiles.Count == 0)
        {
            return 0;
        }

        foreach (StoredFile storedFile in unusedFiles)
        {
            storedFileRepository.Remove(storedFile);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (StoredFile storedFile in unusedFiles)
        {
            await fileStore.DeleteAsync(storedFile.StoragePath, cancellationToken);
        }

        return unusedFiles.Count;
    }
}
