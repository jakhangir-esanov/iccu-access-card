namespace Iccu.UnitTests.Domain;

using Iccu.Domain.Common.Enums;
using Iccu.Domain.RegistrationRequests;

public class RegistrationRequestTests
{
    private static readonly DateTime SubmittedAt = new(2026, 9, 26, 7, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Submit_Should_CreateAPendingRequest()
    {
        RegistrationRequest request = TestData.SubmittedPupil(SubmittedAt);

        Assert.Equal(RegistrationRequestStatus.Pending, request.Status);
        Assert.Equal(SubmittedAt, request.SubmittedAt);
        Assert.Equal(SubmittedAt.AddHours(24), request.ExpiresAt);
    }

    [Fact]
    public void MarkApproved_Should_LinkTheReaderAndTheReviewer()
    {
        RegistrationRequest request = TestData.SubmittedPupil(SubmittedAt);
        var readerId = Guid.CreateVersion7();

        request.MarkApproved(readerId, SubmittedAt.AddMinutes(5), TestData.UserId);

        Assert.Equal(RegistrationRequestStatus.Approved, request.Status);
        Assert.Equal(readerId, request.ReaderId);
        Assert.Equal(TestData.UserId, request.ReviewedBy);
        Assert.Equal(SubmittedAt.AddMinutes(5), request.ReviewedAt);
    }

    [Fact]
    public void MarkRejected_Should_StoreTheReason()
    {
        RegistrationRequest request = TestData.SubmittedPupil(SubmittedAt);

        request.MarkRejected("Rasm aniq emas", SubmittedAt.AddMinutes(3), TestData.UserId);

        Assert.Equal(RegistrationRequestStatus.Rejected, request.Status);
        Assert.Equal("Rasm aniq emas", request.RejectionReason);
    }

    [Fact]
    public void Details_Should_ReturnTheStoredPersonDetails()
    {
        RegistrationRequest request = TestData.SubmittedPupil(SubmittedAt);

        Assert.Equal(request.Category, request.Details.Category);
        Assert.Equal(Gender.Female, request.Details.Gender);
        Assert.Equal(Citizenship.Uzbekistan, request.Details.Citizenship);
    }
}
