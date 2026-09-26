namespace Iccu.UnitTests.RegistrationRequests;

using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Iccu.Application.RegistrationRequests.RejectRegistrationRequest;
using Iccu.Application.RegistrationRequests.UpdateRegistrationRequest;
using Iccu.Domain.RegistrationRequests;

public class ReviewRegistrationRequestCommandHandlerTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeRegistrationRequestRepository _requests = new();
    private readonly FakeClock _clock = new();

    private RegistrationRequest Submit(TimeSpan age)
    {
        RegistrationRequest request = TestData.SubmittedPupil(_clock.UtcNow - age);
        _requests.Insert(request);
        return request;
    }

    private RejectRegistrationRequestCommandHandler RejectHandler() =>
        new(_unitOfWork, _requests, new FakeCurrentUser(TestData.UserId), _clock);

    private UpdateRegistrationRequestCommandHandler UpdateHandler() => new(_unitOfWork, _requests, _clock);

    [Fact]
    public async Task Reject_Should_TrimTheReason()
    {
        RegistrationRequest request = Submit(TimeSpan.FromMinutes(5));

        var result = await RejectHandler().Handle(
            new RejectRegistrationRequestCommand(request.Id, "  Rasm aniq emas  "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RegistrationRequestStatus.Rejected, request.Status);
        Assert.Equal("Rasm aniq emas", request.RejectionReason);
    }

    [Fact]
    public async Task Reject_WhenAlreadyRejected_ReturnsNotPending()
    {
        RegistrationRequest request = Submit(TimeSpan.FromMinutes(5));
        request.MarkRejected("Birinchi", _clock.UtcNow, TestData.UserId);

        var result = await RejectHandler().Handle(
            new RejectRegistrationRequestCommand(request.Id, "Ikkinchi"), CancellationToken.None);

        Assert.Equal(RegistrationRequestErrors.NotPending, result.Error);
        Assert.Equal("Birinchi", request.RejectionReason);
    }

    [Fact]
    public async Task Update_Should_NormalizeTheNewDetails()
    {
        RegistrationRequest request = Submit(TimeSpan.FromMinutes(5));

        var result = await UpdateHandler().Handle(
            new UpdateRegistrationRequestCommand(request.Id, TestData.Student()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReaderCategory.Student, request.Category);
        Assert.Equal("AA1234567", request.DocumentNumber);
        Assert.Equal("+998901234567", request.Phone);
    }

    [Fact]
    public async Task Update_AfterTheDeadline_ReturnsExpired()
    {
        RegistrationRequest request = Submit(TimeSpan.FromHours(25));

        var result = await UpdateHandler().Handle(
            new UpdateRegistrationRequestCommand(request.Id, TestData.Student()), CancellationToken.None);

        Assert.Equal(RegistrationRequestErrors.Expired, result.Error);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
