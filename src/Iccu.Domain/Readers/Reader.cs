namespace Iccu.Domain.Readers;

using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;

public sealed class Reader
{
    private Reader()
    {
    }

    public Guid Id { get; private set; }

    public int CardNumber { get; private set; }

    public ReaderCategory Category { get; private set; }

    public string LastName { get; private set; } = null!;

    public string FirstName { get; private set; } = null!;

    public string? MiddleName { get; private set; }

    public DateOnly BirthDate { get; private set; }

    public Gender? Gender { get; private set; }

    public Citizenship? Citizenship { get; private set; }

    public string Phone { get; private set; } = null!;

    public Guid PhotoFileId { get; private set; }

    public RegistrationSource Source { get; private set; }

    public DateOnly IssuedOn { get; private set; }

    public DateOnly ExpiresOn { get; private set; }

    public int PrintCount { get; private set; }

    public DateTime? LastPrintedAt { get; private set; }

    public bool IsKohaSynced { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static Reader Register(
        PersonDetails details,
        Guid photoFileId,
        RegistrationSource source,
        DateOnly issuedOn,
        DateOnly expiresOn,
        DateTime utcNow,
        Guid userId)
    {
        var reader = new Reader
        {
            Id = Guid.CreateVersion7(utcNow),
            PhotoFileId = photoFileId,
            Source = source,
            IssuedOn = issuedOn,
            ExpiresOn = expiresOn,
            CreatedAt = utcNow,
            CreatedBy = userId
        };

        reader.SetDetails(details);

        return reader;
    }

    public void UpdateDetails(PersonDetails details, DateTime utcNow)
    {
        SetDetails(details);
        UpdatedAt = utcNow;
    }

    public void ReplacePhoto(Guid photoFileId, DateTime utcNow)
    {
        PhotoFileId = photoFileId;
        UpdatedAt = utcNow;
    }

    public void RenewCard(DateOnly issuedOn, DateOnly expiresOn, DateTime utcNow)
    {
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
        UpdatedAt = utcNow;
    }

    public void RecordPrint(DateTime utcNow)
    {
        PrintCount++;
        LastPrintedAt = utcNow;
    }

    public void MarkKohaSynced()
    {
        IsKohaSynced = true;
    }

    public void MarkDeleted(DateTime utcNow)
    {
        DeletedAt = utcNow;
    }

    private void SetDetails(PersonDetails details)
    {
        Category = details.Category;
        LastName = details.LastName;
        FirstName = details.FirstName;
        MiddleName = details.MiddleName;
        BirthDate = details.BirthDate;
        Gender = details.Gender;
        Citizenship = details.Citizenship;
        Phone = details.Phone;
    }
}
