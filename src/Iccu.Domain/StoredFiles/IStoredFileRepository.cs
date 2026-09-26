namespace Iccu.Domain.StoredFiles;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredFile>> GetUnusedAsync(
        DateTime createdBefore,
        int limit,
        CancellationToken cancellationToken = default);

    void Insert(StoredFile storedFile);

    void Remove(StoredFile storedFile);
}
