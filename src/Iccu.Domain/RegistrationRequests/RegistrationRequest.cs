namespace Iccu.Domain.RegistrationRequests;

using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;

public sealed class RegistrationRequest
{
    private RegistrationRequest()
    {
    }

    public Guid Id { get; private set; }

    public int Code { get; private set; }

    public RegistrationRequestStatus Status { get; private set; }

    public ReaderCategory Category { get; private set; }

    public string LastName { get; private set; } = null!;

    public string FirstName { get; private set; } = null!;

    public string? MiddleName { get; private set; }

    public DateOnly BirthDate { get; private set; }

    public string Phone { get; private set; } = null!;

    public DocumentType DocumentType { get; private set; }

    public string DocumentNumber { get; private set; } = null!;

    public Guid PhotoFileId { get; private set; }

    public DateTime SubmittedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? ReviewedAt { get; private set; }

    public Guid? ReviewedBy { get; private set; }

    public string? RejectionReason { get; private set; }

    public Guid? ReaderId { get; private set; }

    public PersonDetails Details => new(
        Category,
        LastName,
        FirstName,
        MiddleName,
        BirthDate,
        Phone,
        DocumentType,
        DocumentNumber);

    public static RegistrationRequest Submit(
        PersonDetails details,
        Guid photoFileId,
        DateTime submittedAt,
        DateTime expiresAt)
    {
        var request = new RegistrationRequest
        {
            Id = Guid.CreateVersion7(submittedAt),
            Status = RegistrationRequestStatus.Pending,
            PhotoFileId = photoFileId,
            SubmittedAt = submittedAt,
            ExpiresAt = expiresAt
        };

        request.UpdateDetails(details);

        return request;
    }

    public void UpdateDetails(PersonDetails details)
    {
        Category = details.Category;
        LastName = details.LastName;
        FirstName = details.FirstName;
        MiddleName = details.MiddleName;
        BirthDate = details.BirthDate;
        Phone = details.Phone;
        DocumentType = details.DocumentType;
        DocumentNumber = details.DocumentNumber;
    }

    public void MarkApproved(Guid readerId, DateTime utcNow, Guid userId)
    {
        Status = RegistrationRequestStatus.Approved;
        ReaderId = readerId;
        ReviewedAt = utcNow;
        ReviewedBy = userId;
    }

    public void MarkRejected(string reason, DateTime utcNow, Guid userId)
    {
        Status = RegistrationRequestStatus.Rejected;
        RejectionReason = reason;
        ReviewedAt = utcNow;
        ReviewedBy = userId;
    }

    public void MarkExpired()
    {
        Status = RegistrationRequestStatus.Expired;
    }
}
