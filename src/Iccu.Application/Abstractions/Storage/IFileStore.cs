namespace Iccu.Application.Abstractions.Storage;

public interface IFileStore
{
    Task SaveAsync(string storagePath, Stream content, CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
