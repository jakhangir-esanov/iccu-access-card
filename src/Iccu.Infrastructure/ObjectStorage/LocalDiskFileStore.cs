namespace Iccu.Infrastructure.ObjectStorage;

using Microsoft.Extensions.Options;
using Iccu.Application.Abstractions.Storage;

internal sealed class LocalDiskFileStore(IOptions<StorageOptions> options) : IFileStore
{
    private readonly string _rootPath = Path.GetFullPath(options.Value.RootPath);

    public async Task SaveAsync(string storagePath, Stream content, CancellationToken cancellationToken = default)
    {
        string path = ResolvePath(storagePath);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using FileStream target = File.Create(path);

        await content.CopyToAsync(target, cancellationToken);
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        string path = ResolvePath(storagePath);

        return Task.FromResult<Stream?>(File.Exists(path) ? File.OpenRead(path) : null);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        File.Delete(ResolvePath(storagePath));

        return Task.CompletedTask;
    }

    private string ResolvePath(string storagePath) => Path.Combine(_rootPath, storagePath);
}
