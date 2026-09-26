namespace Iccu.UnitTests.Infrastructure;

using Iccu.Infrastructure.ObjectStorage;
using Microsoft.Extensions.Options;

public sealed class LocalDiskFileStoreTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"iccu-files-{Guid.NewGuid():N}");
    private readonly LocalDiskFileStore _store;

    public LocalDiskFileStoreTests()
    {
        _store = new LocalDiskFileStore(Options.Create(new StorageOptions { RootPath = _root }));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_Should_WriteTheContentUnderTheRoot()
    {
        await _store.SaveAsync("photo.jpg", new MemoryStream([1, 2, 3]));

        await using Stream? content = await _store.OpenReadAsync("photo.jpg");

        Assert.NotNull(content);
        Assert.Equal(3, content.Length);
        Assert.True(File.Exists(Path.Combine(_root, "photo.jpg")));
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveTheFile()
    {
        await _store.SaveAsync("photo.jpg", new MemoryStream([1]));

        await _store.DeleteAsync("photo.jpg");

        Assert.Null(await _store.OpenReadAsync("photo.jpg"));
    }

    [Fact]
    public async Task OpenReadAsync_WhenTheFileIsMissing_ReturnsNull()
    {
        Assert.Null(await _store.OpenReadAsync("missing.jpg"));
    }
}
