namespace Iccu.UnitTests.RegistrationRequests;

using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Iccu.Domain.RegistrationRequests;
using Iccu.Application.RegistrationRequests.ExpireRegistrationRequests;

public class ExpireRegistrationRequestsCommandHandlerTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeRegistrationRequestRepository _requests = new();
    private readonly FakeClock _clock = new();

    private ExpireRegistrationRequestsCommandHandler Handler() => new(_unitOfWork, _requests, _clock);

    private RegistrationRequest Submit(TimeSpan age)
    {
        RegistrationRequest request = TestData.SubmittedPupil(_clock.UtcNow - age);
        _requests.Insert(request);
        return request;
    }

    [Fact]
    public async Task Handle_Should_ExpireOnlyStaleRequests()
    {
        RegistrationRequest stale = Submit(TimeSpan.FromHours(25));
        RegistrationRequest fresh = Submit(TimeSpan.FromHours(2));

        var result = await Handler().Handle(new ExpireRegistrationRequestsCommand(), CancellationToken.None);

        Assert.Equal(1, result.Data);
        Assert.Equal(RegistrationRequestStatus.Expired, stale.Status);
        Assert.Equal(RegistrationRequestStatus.Pending, fresh.Status);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenNothingIsStale_DoesNotSave()
    {
        Submit(TimeSpan.FromHours(1));

        var result = await Handler().Handle(new ExpireRegistrationRequestsCommand(), CancellationToken.None);

        Assert.Equal(0, result.Data);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
