namespace Iccu.UnitTests.Koha;

using Iccu.Domain.Readers;
using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Koha.MarkKohaReaderSynced;

public class MarkKohaReaderSyncedCommandHandlerTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeReaderRepository _readers = new();
    private readonly FakeClock _clock = new();

    private MarkKohaReaderSyncedCommandHandler Handler() => new(
        _unitOfWork,
        _readers);

    private Reader CreateReader() => Reader.Register(
        TestData.Student(),
        Guid.NewGuid(),
        RegistrationSource.Reception,
        _clock.Today,
        _clock.Today.AddYears(2),
        _clock.UtcNow,
        TestData.UserId);

    [Fact]
    public async Task Handle_WhenReaderExists_MarksSyncedAndSaves()
    {
        Reader reader = CreateReader();
        _readers.Insert(reader);

        var result = await Handler().Handle(new MarkKohaReaderSyncedCommand(reader.CardNumber), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(reader.IsKohaSynced);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenReaderIsAlreadySynced_ReturnsSuccess()
    {
        Reader reader = CreateReader();
        reader.MarkKohaSynced();
        _readers.Insert(reader);

        var result = await Handler().Handle(new MarkKohaReaderSyncedCommand(reader.CardNumber), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(reader.IsKohaSynced);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenReaderNotFound_ReturnsNotFound()
    {
        var result = await Handler().Handle(new MarkKohaReaderSyncedCommand(9999999), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ReaderErrors.NotFound, result.Error);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenReaderIsDeleted_ReturnsNotFound()
    {
        Reader reader = CreateReader();
        reader.MarkDeleted(_clock.UtcNow);
        _readers.Insert(reader);

        var result = await Handler().Handle(new MarkKohaReaderSyncedCommand(reader.CardNumber), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ReaderErrors.NotFound, result.Error);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
