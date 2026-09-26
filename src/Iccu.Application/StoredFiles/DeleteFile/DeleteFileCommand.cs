namespace Iccu.Application.StoredFiles.DeleteFile;

using Iccu.Domain.Common;
using Iccu.Domain.StoredFiles;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Storage;

public sealed record DeleteFileCommand(Guid Id) : ICommand;

internal sealed class DeleteFileCommandHandler(
    IUnitOfWork unitOfWork,
    IFileStore fileStore,
    IStoredFileRepository storedFileRepository) : ICommandHandler<DeleteFileCommand>
{
    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        StoredFile? storedFile = await storedFileRepository.GetAsync(request.Id, cancellationToken);
        if (storedFile is null)
        {
            return Result.Failure(StoredFileErrors.NotFound);
        }

        if (await storedFileRepository.IsInUseAsync(request.Id, cancellationToken))
        {
            return Result.Failure(StoredFileErrors.InUse);
        }

        storedFileRepository.Remove(storedFile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await fileStore.DeleteAsync(storedFile.StoragePath, cancellationToken);

        return Result.Success();
    }
}
