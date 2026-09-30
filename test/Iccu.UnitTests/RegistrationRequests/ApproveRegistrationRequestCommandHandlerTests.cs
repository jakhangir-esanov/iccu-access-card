namespace Iccu.UnitTests.RegistrationRequests;

using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Iccu.Application.RegistrationRequests.ApproveRegistrationRequest;
using Iccu.Domain.Readers;
using Iccu.Domain.RegistrationRequests;

public class ApproveRegistrationRequestCommandHandlerTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeRegistrationRequestRepository _requests = new();
    private readonly FakeReaderRepository _readers = new();
    private readonly FakeClock _clock = new();

    private ApproveRegistrationRequestCommandHandler Handler() => new(
        _unitOfWork,
        _requests,
        _readers,
        new FakeCurrentUser(TestData.UserId),
        _clock);

    private RegistrationRequest SubmitPupil()
    {
        RegistrationRequest request = TestData.SubmittedPupil(_clock.UtcNow.AddMinutes(-10));
        _requests.Insert(request);
        return request;
    }

    [Fact]
    public async Task Handle_Should_CreateASelfServiceReaderAndCloseTheRequest()
    {
        RegistrationRequest request = SubmitPupil();

        var result = await Handler().Handle(new ApproveRegistrationRequestCommand(request.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Reader reader = Assert.Single(_readers.Readers);
        Assert.Equal(result.Data.ReaderId, reader.Id);
        Assert.Equal(RegistrationSource.SelfService, reader.Source);
        Assert.Equal(RegistrationRequestStatus.Approved, request.Status);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenThePhoneAlreadyBelongsToAReader_ReturnsConflict()
    {
        RegistrationRequest request = SubmitPupil();
        _readers.Insert(TestData.RegisteredReader(TestData.Student(phone: "93 555 66 77"), _clock.UtcNow));

        var result = await Handler().Handle(new ApproveRegistrationRequestCommand(request.Id), CancellationToken.None);

        Assert.Equal(ReaderErrors.PhoneAlreadyRegistered, result.Error);
        Assert.Equal(RegistrationRequestStatus.Pending, request.Status);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_Should_IssueACardValidForTwoYears()
    {
        RegistrationRequest request = SubmitPupil();

        await Handler().Handle(new ApproveRegistrationRequestCommand(request.Id), CancellationToken.None);

        Reader reader = Assert.Single(_readers.Readers);
        Assert.Equal(_clock.Today, reader.IssuedOn);
        Assert.Equal(_clock.Today.AddYears(2), reader.ExpiresOn);
    }

    [Fact]
    public async Task Handle_AfterTheDeadline_ReturnsExpiredAndCreatesNoReader()
    {
        RegistrationRequest request = SubmitPupil();
        _clock.UtcNow = request.ExpiresAt.AddSeconds(1);

        var result = await Handler().Handle(new ApproveRegistrationRequestCommand(request.Id), CancellationToken.None);

        Assert.Equal(RegistrationRequestErrors.Expired, result.Error);
        Assert.Empty(_readers.Readers);
    }

    [Fact]
    public async Task Handle_WhenAlreadyApproved_ReturnsNotPending()
    {
        RegistrationRequest request = SubmitPupil();
        await Handler().Handle(new ApproveRegistrationRequestCommand(request.Id), CancellationToken.None);

        var result = await Handler().Handle(new ApproveRegistrationRequestCommand(request.Id), CancellationToken.None);

        Assert.Equal(RegistrationRequestErrors.NotPending, result.Error);
        Assert.Single(_readers.Readers);
    }

    [Fact]
    public async Task Handle_WhenTheRequestDoesNotExist_ReturnsNotFound()
    {
        var result = await Handler().Handle(new ApproveRegistrationRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(RegistrationRequestErrors.NotFound, result.Error);
    }
}
