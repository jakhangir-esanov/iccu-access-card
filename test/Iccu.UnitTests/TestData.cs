namespace Iccu.UnitTests;

using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Validation;
using Iccu.Domain.Readers;
using Iccu.Domain.StoredFiles;
using Iccu.Domain.RegistrationRequests;

internal static class TestData
{
    public static readonly Guid UserId = Guid.Parse("01a0da36-9ee4-743b-9eae-d23a48b1ad3f");

    public static PersonDetails Student(
        string phone = "90 123 45 67",
        string lastName = " Karimov ",
        Citizenship? citizenship = Citizenship.Uzbekistan) =>
        new(
            ReaderCategory.Student,
            lastName,
            "Ali",
            " ",
            new DateOnly(2003, 5, 14),
            Gender.Male,
            citizenship,
            phone);

    public static PersonDetails Pupil() =>
        new(
            ReaderCategory.Pupil,
            "To‘xtayeva",
            "Gulnoza",
            null,
            new DateOnly(2014, 2, 10),
            Gender.Female,
            Citizenship.Uzbekistan,
            "+998 93 555 66 77");

    public static StoredFile Photo(DateTime createdAt) =>
        StoredFile.Create(".jpg", "image/jpeg", 2048, createdAt);

    public static RegistrationRequest SubmittedPupil(DateTime submittedAt, Guid? photoFileId = null) =>
        RegistrationRequest.Submit(
            Pupil().Normalized(),
            photoFileId ?? Guid.NewGuid(),
            submittedAt,
            submittedAt.AddHours(24));

    public static Reader RegisteredReader(PersonDetails details, DateTime utcNow)
    {
        DateOnly issuedOn = DateOnly.FromDateTime(utcNow);

        return Reader.Register(
            details.Normalized(),
            Guid.NewGuid(),
            RegistrationSource.Reception,
            issuedOn,
            issuedOn.AddYears(2),
            utcNow,
            UserId);
    }
}
