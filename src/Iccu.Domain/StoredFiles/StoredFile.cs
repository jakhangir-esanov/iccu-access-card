namespace Iccu.Domain.StoredFiles;

public sealed class StoredFile
{
    private StoredFile()
    {
    }

    public Guid Id { get; private set; }

    public string OriginalName { get; private set; } = null!;

    public string Extension { get; private set; } = null!;

    public string ContentType { get; private set; } = null!;

    public long Size { get; private set; }

    public string StoragePath { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public static StoredFile Create(
        string originalName,
        string extension,
        string contentType,
        long size,
        DateTime utcNow)
    {
        var id = Guid.NewGuid();

        return new StoredFile
        {
            Id = id,
            OriginalName = originalName,
            Extension = extension,
            ContentType = contentType,
            Size = size,
            StoragePath = $"{id}{extension}".ToLowerInvariant(),
            CreatedAt = utcNow
        };
    }
}
