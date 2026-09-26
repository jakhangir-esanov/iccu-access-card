namespace Iccu.UnitTests.Readers;

using Iccu.Domain.Readers;
using Iccu.Domain.Common;
using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Readers.CreateReader;

public class CreateReaderCommandValidatorTests
{
    private readonly CreateReaderCommandValidator _validator = new(new FakeClock());

    private bool IsValid(PersonDetails details, Guid? photoFileId = null) =>
        _validator.Validate(new CreateReaderCommand(details, photoFileId ?? Guid.NewGuid())).IsValid;

    private string[] ErrorCodes(PersonDetails details) =>
        [.. _validator.Validate(new CreateReaderCommand(details, Guid.NewGuid())).Errors.Select(e => e.ErrorCode)];

    [Fact]
    public void Validate_WhenEverythingIsFilled_Passes()
    {
        Assert.True(IsValid(TestData.Student()));
        Assert.True(IsValid(TestData.Pupil()));
    }

    [Theory]
    [InlineData("O‘rinboyev")]
    [InlineData("Oʻrinboyev")]
    [InlineData("O'rinboyev")]
    [InlineData("Абдуллаев")]
    [InlineData("Karimova-Sodiqova")]
    public void Validate_Should_AcceptUzbekLatinAndCyrillicNames(string lastName)
    {
        Assert.True(IsValid(TestData.Student(lastName: lastName)));
    }

    [Fact]
    public void Validate_WhenTheLastNameIsEmpty_ReportsASingleError()
    {
        string[] codes = ErrorCodes(TestData.Student(lastName: string.Empty));

        Assert.Equal(["NotEmptyValidator"], codes);
    }

    [Fact]
    public void Validate_WhenTheNameContainsDigits_Fails()
    {
        Assert.False(IsValid(TestData.Student(lastName: "Karimov2")));
    }

    [Fact]
    public void Validate_WhenThePhoneIsNotUzbek_ReportsInvalidPhone()
    {
        Assert.Contains(ReaderErrors.InvalidPhone.Code, ErrorCodes(TestData.Student(phone: "+7 901 234 56 78")));
    }

    [Fact]
    public void Validate_WhenThePassportIsMalformed_ReportsInvalidPassport()
    {
        Assert.Contains(ReaderErrors.InvalidPassport.Code, ErrorCodes(TestData.Student(documentNumber: "A123")));
    }

    [Fact]
    public void Validate_WhenTheBirthDateIsInTheFuture_ReportsInvalidBirthDate()
    {
        PersonDetails details = TestData.Student() with { BirthDate = new DateOnly(2030, 1, 1) };

        Assert.Contains(ReaderErrors.InvalidBirthDate.Code, ErrorCodes(details));
    }

    [Fact]
    public void Validate_WhenTheCategoryIsUnknown_Fails()
    {
        PersonDetails details = TestData.Student() with { Category = (ReaderCategory)99 };

        Assert.False(IsValid(details));
    }

    [Fact]
    public void Validate_WhenThePhotoIsMissing_Fails()
    {
        Assert.False(IsValid(TestData.Student(), Guid.Empty));
    }
}
