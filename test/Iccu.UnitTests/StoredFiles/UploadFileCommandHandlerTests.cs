namespace Iccu.UnitTests.StoredFiles;

using Iccu.Domain.Common;
using Iccu.UnitTests.Fakes;
using Iccu.Domain.StoredFiles;
using Iccu.Application.StoredFiles.UploadFile;

public class UploadFileCommandHandlerTests
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];
    private static readonly byte[] Webp = [.. "RIFF"u8, 0x24, 0x00, 0x00, 0x00, .. "WEBPVP8 "u8];
    private static readonly byte[] Pdf = [.. "%PDF-1.7 not a photo"u8];

    private readonly FakeFileStore _fileStore = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeStoredFileRepository _storedFiles = new();
    private readonly FakeClock _clock = new();

    private UploadFileCommandHandler Handler() => new(_fileStore, _unitOfWork, _clock, _storedFiles);

    private Task<Result<Guid>> Upload(string fileName, byte[] content, long? size = null) =>
        Handler().Handle(
            new UploadFileCommand(fileName, size ?? content.Length, new MemoryStream(content)),
            CancellationToken.None);

    public static TheoryData<string, byte[], string> SupportedPhotos => new()
    {
        { "Photo.JPG", Jpeg, "image/jpeg" },
        { "photo.jpeg", Jpeg, "image/jpeg" },
        { "photo.png", Png, "image/png" },
        { "photo.webp", Webp, "image/webp" }
    };

    [Theory]
    [MemberData(nameof(SupportedPhotos))]
    public async Task Handle_ForASupportedPhoto_StoresTheFileAndItsRecord(string fileName, byte[] content, string contentType)
    {
        var result = await Upload(fileName, content);

        Assert.True(result.IsSuccess);
        StoredFile storedFile = Assert.Single(_storedFiles.Files);
        Assert.Equal(result.Data, storedFile.Id);
        Assert.Equal(fileName, storedFile.OriginalName);
        Assert.Equal(contentType, storedFile.ContentType);
        Assert.Equal($"{storedFile.Id}{Path.GetExtension(fileName).ToLowerInvariant()}", storedFile.StoragePath);
        Assert.Equal(content, _fileStore.Files[storedFile.StoragePath]);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ForAnEmptyFile_ReturnsEmpty()
    {
        var result = await Upload("photo.jpg", [], size: 0);

        Assert.Equal(StoredFileErrors.Empty, result.Error);
    }

    [Theory]
    [InlineData("scan.pdf")]
    [InlineData("photo.gif")]
    [InlineData("photo")]
    public async Task Handle_ForAnUnsupportedExtension_ReturnsUnsupportedContent(string fileName)
    {
        var result = await Upload(fileName, Pdf);

        Assert.Equal(StoredFileErrors.UnsupportedContent, result.Error);
    }

    [Fact]
    public async Task Handle_ForAFileOverEightMegabytes_ReturnsTooLarge()
    {
        var result = await Upload("photo.jpg", Jpeg, size: 8L * 1024 * 1024 + 1);

        Assert.Equal(StoredFileErrors.TooLarge, result.Error);
    }

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.png")]
    [InlineData("photo.webp")]
    public async Task Handle_WhenTheContentDoesNotMatchTheExtension_ReturnsContentMismatchAndStoresNothing(string fileName)
    {
        var result = await Upload(fileName, Pdf);

        Assert.Equal(StoredFileErrors.ContentMismatch, result.Error);
        Assert.Empty(_fileStore.Files);
        Assert.Empty(_storedFiles.Files);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
