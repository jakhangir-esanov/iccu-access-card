namespace Iccu.UnitTests.Domain;

using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Iccu.Domain.Readers;

public class ReaderTests
{
    private static readonly DateOnly Today = new(2026, 9, 26);
    private static readonly DateTime UtcNow = new(2026, 9, 26, 7, 0, 0, DateTimeKind.Utc);
    private static readonly Guid PhotoFileId = Guid.NewGuid();

    private static readonly PersonDetails Details = new(
        ReaderCategory.Student,
        "Karimov",
        "Ali",
        null,
        new DateOnly(2003, 5, 14),
        Gender.Male,
        Citizenship.Uzbekistan,
        "+998901234567");

    private static Reader Register() => Reader.Register(
        Details,
        PhotoFileId,
        RegistrationSource.Reception,
        Today,
        Today.AddYears(2),
        UtcNow,
        TestData.UserId);

    [Fact]
    public void Register_Should_StoreTheGivenDetailsAndCardPeriod()
    {
        Reader reader = Register();

        Assert.Equal(Details, new PersonDetails(
            reader.Category,
            reader.LastName,
            reader.FirstName,
            reader.MiddleName,
            reader.BirthDate,
            reader.Gender,
            reader.Citizenship,
            reader.Phone));
        Assert.Equal(Today, reader.IssuedOn);
        Assert.Equal(Today.AddYears(2), reader.ExpiresOn);
        Assert.Equal(TestData.UserId, reader.CreatedBy);
        Assert.Equal(UtcNow, reader.CreatedAt);
    }

    [Fact]
    public void UpdateDetails_Should_ReplaceTheDetailsAndStampTheChange()
    {
        Reader reader = Register();

        reader.UpdateDetails(Details with { Category = ReaderCategory.Master }, UtcNow.AddDays(1));

        Assert.Equal(ReaderCategory.Master, reader.Category);
        Assert.Equal(UtcNow.AddDays(1), reader.UpdatedAt);
    }

    [Fact]
    public void RenewCard_Should_StoreTheNewPeriod()
    {
        Reader reader = Register();
        var renewedOn = new DateOnly(2028, 10, 1);

        reader.RenewCard(renewedOn, renewedOn.AddYears(2), UtcNow.AddYears(2));

        Assert.Equal(renewedOn, reader.IssuedOn);
        Assert.Equal(new DateOnly(2030, 10, 1), reader.ExpiresOn);
    }

    [Fact]
    public void ReplacePhoto_Should_PointAtTheNewFile()
    {
        Reader reader = Register();
        var newPhotoFileId = Guid.NewGuid();

        reader.ReplacePhoto(newPhotoFileId, UtcNow.AddHours(1));

        Assert.Equal(newPhotoFileId, reader.PhotoFileId);
        Assert.Equal(UtcNow.AddHours(1), reader.UpdatedAt);
    }

    [Fact]
    public void RecordPrint_Should_CountEveryPrint()
    {
        Reader reader = Register();

        reader.RecordPrint(UtcNow);
        reader.RecordPrint(UtcNow.AddMinutes(1));

        Assert.Equal(2, reader.PrintCount);
        Assert.Equal(UtcNow.AddMinutes(1), reader.LastPrintedAt);
    }

    [Fact]
    public void MarkDeleted_Should_StampTheDeletionTime()
    {
        Reader reader = Register();

        reader.MarkDeleted(UtcNow);

        Assert.Equal(UtcNow, reader.DeletedAt);
    }

    [Fact]
    public void MarkKohaSynced_Should_SetIsKohaSyncedToTrue()
    {
        Reader reader = Register();

        Assert.False(reader.IsKohaSynced);

        reader.MarkKohaSynced();

        Assert.True(reader.IsKohaSynced);
    }

    [Fact]
    public void MarkKohaSynced_WhenAlreadySynced_RemainsTrue()
    {
        Reader reader = Register();

        reader.MarkKohaSynced();
        reader.MarkKohaSynced();

        Assert.True(reader.IsKohaSynced);
    }
}
