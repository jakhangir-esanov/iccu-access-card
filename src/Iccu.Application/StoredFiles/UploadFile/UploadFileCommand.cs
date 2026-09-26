namespace Iccu.Application.StoredFiles.UploadFile;

using Iccu.Domain.Common;
using Iccu.Domain.StoredFiles;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Storage;

public sealed record UploadFileCommand(string FileName, long Size, Stream Content) : ICommand<Guid>;

internal sealed class UploadFileCommandHandler(
    IFileStore fileStore,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IStoredFileRepository storedFileRepository) : ICommandHandler<UploadFileCommand, Guid>
{
    private const long MaxSize = 8L * 1024 * 1024;
    private const int HeaderLength = 16;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<Result<Guid>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        if (request.Size <= 0)
        {
            return Result.Failure<Guid>(StoredFileErrors.Empty);
        }

        string extension = Path.GetExtension(request.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            return Result.Failure<Guid>(StoredFileErrors.UnsupportedContent);
        }

        if (request.Size > MaxSize)
        {
            return Result.Failure<Guid>(StoredFileErrors.TooLarge);
        }

        byte[] header = new byte[HeaderLength];
        int headerLength = await request.Content.ReadAtLeastAsync(
            header, header.Length, throwOnEndOfStream: false, cancellationToken);
        request.Content.Position = 0;

        if (!MatchesSignature(extension, header.AsSpan(0, headerLength)))
        {
            return Result.Failure<Guid>(StoredFileErrors.ContentMismatch);
        }

        var storedFile = StoredFile.Create(
            request.FileName,
            extension,
            ContentTypeFor(extension),
            request.Size,
            dateTimeProvider.UtcNow);

        await fileStore.SaveAsync(storedFile.StoragePath, request.Content, cancellationToken);

        storedFileRepository.Insert(storedFile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return storedFile.Id;
    }

    private static string ContentTypeFor(string extension) => extension switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "image/jpeg"
    };

    private static bool MatchesSignature(string extension, ReadOnlySpan<byte> header) => extension switch
    {
        ".jpg" or ".jpeg" => header.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]),
        ".png" => header.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        ".webp" => header.Length >= 12 && header.StartsWith("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8),
        _ => false
    };
}
