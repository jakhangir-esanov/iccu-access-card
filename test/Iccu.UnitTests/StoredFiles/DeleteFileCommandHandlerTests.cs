namespace Iccu.UnitTests.StoredFiles;

using Iccu.UnitTests.Fakes;
using Iccu.Domain.StoredFiles;
using Iccu.Application.StoredFiles.DeleteFile;
using Iccu.Application.StoredFiles.DeleteUnusedFiles;

public class DeleteFileCommandHandlerTests
{
    private readonly FakeFileStore _fileStore = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeStoredFileRepository _storedFiles = new();
    private readonly FakeClock _clock = new();

    private StoredFile Store(TimeSpan age)
    {
        StoredFile storedFile = TestData.Photo(_clock.UtcNow - age);
        _storedFiles.Insert(storedFile);
        _fileStore.Files[storedFile.StoragePath] = [1, 2, 3];
        return storedFile;
    }

    [Fact]
    public async Task DeleteFile_WhenUnused_RemovesTheRecordAndTheFile()
    {
        StoredFile storedFile = Store(TimeSpan.Zero);

        var result = await new DeleteFileCommandHandler(_unitOfWork, _fileStore, _storedFiles)
            .Handle(new DeleteFileCommand(storedFile.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_storedFiles.Files);
        Assert.Equal([storedFile.StoragePath], _fileStore.Deleted);
    }

    [Fact]
    public async Task DeleteFile_WhenStillUsed_ReturnsInUseAndKeepsTheFile()
    {
        StoredFile storedFile = Store(TimeSpan.Zero);
        _storedFiles.UsedFileIds.Add(storedFile.Id);

        var result = await new DeleteFileCommandHandler(_unitOfWork, _fileStore, _storedFiles)
            .Handle(new DeleteFileCommand(storedFile.Id), CancellationToken.None);

        Assert.Equal(StoredFileErrors.InUse, result.Error);
        Assert.Single(_storedFiles.Files);
        Assert.Empty(_fileStore.Deleted);
    }

    [Fact]
    public async Task DeleteFile_WhenMissing_ReturnsNotFound()
    {
        var result = await new DeleteFileCommandHandler(_unitOfWork, _fileStore, _storedFiles)
            .Handle(new DeleteFileCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task DeleteUnusedFiles_Should_KeepUsedAndRecentFiles()
    {
        StoredFile unused = Store(TimeSpan.FromHours(25));
        StoredFile used = Store(TimeSpan.FromHours(25));
        StoredFile recent = Store(TimeSpan.FromHours(1));
        _storedFiles.UsedFileIds.Add(used.Id);

        var result = await new DeleteUnusedFilesCommandHandler(_unitOfWork, _fileStore, _storedFiles, _clock)
            .Handle(new DeleteUnusedFilesCommand(), CancellationToken.None);

        Assert.Equal(1, result.Data);
        Assert.Equal([used, recent], _storedFiles.Files);
        Assert.Equal([unused.StoragePath], _fileStore.Deleted);
    }
}
