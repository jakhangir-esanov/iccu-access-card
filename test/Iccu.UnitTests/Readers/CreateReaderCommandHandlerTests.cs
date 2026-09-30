namespace Iccu.UnitTests.Readers;

using Iccu.Domain.Readers;
using Iccu.UnitTests.Fakes;
using Iccu.Domain.StoredFiles;
using System.Globalization;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Readers.CreateReader;

public class CreateReaderCommandHandlerTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeReaderRepository _readers = new();
    private readonly FakeStoredFileRepository _storedFiles = new();
    private readonly FakeClock _clock = new();
    private readonly StoredFile _photo;

    public CreateReaderCommandHandlerTests()
    {
        _photo = TestData.Photo(_clock.UtcNow);
        _storedFiles.Insert(_photo);
    }

    private CreateReaderCommandHandler Handler() => new(
        _unitOfWork,
        _readers,
        _storedFiles,
        new FakeCurrentUser(TestData.UserId),
        _clock);

    private CreateReaderCommand Command(string phone = "+998901234567") =>
        new(TestData.Student(phone), _photo.Id);

    [Fact]
    public async Task Handle_Should_RegisterAReceptionReaderWithTheUploadedPhoto()
    {
        var result = await Handler().Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Reader reader = Assert.Single(_readers.Readers);
        Assert.Equal(RegistrationSource.Reception, reader.Source);
        Assert.Equal(_photo.Id, reader.PhotoFileId);
        Assert.Equal(_clock.Today, reader.IssuedOn);
        Assert.Equal(_clock.Today.AddYears(2), reader.ExpiresOn);
        Assert.Equal(reader.CardNumber.ToString("D7", CultureInfo.InvariantCulture), result.Data.CardNumber);
        Assert.Equal(7, result.Data.CardNumber.Length);
        Assert.Equal(TestData.UserId, reader.CreatedBy);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenThePhoneIsAlreadyRegistered_ReturnsConflict()
    {
        await Handler().Handle(Command("+998901234567"), CancellationToken.None);

        var result = await Handler().Handle(Command("90 123-45-67"), CancellationToken.None);

        Assert.Equal(ReaderErrors.PhoneAlreadyRegistered, result.Error);
        Assert.Single(_readers.Readers);
    }

    [Fact]
    public async Task Handle_WhenAnotherPhoneIsUsed_RegistersASecondReader()
    {
        await Handler().Handle(Command("+998901234567"), CancellationToken.None);

        var result = await Handler().Handle(Command("+998931112233"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, _readers.Readers.Count);
    }

    [Fact]
    public async Task Handle_WhenThePhotoWasNotUploaded_ReturnsNotFoundAndSavesNothing()
    {
        var command = new CreateReaderCommand(TestData.Student(), Guid.NewGuid());

        var result = await Handler().Handle(command, CancellationToken.None);

        Assert.Equal(StoredFileErrors.NotFound, result.Error);
        Assert.Empty(_readers.Readers);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
