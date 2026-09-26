namespace Iccu.UnitTests.Common;

using Iccu.Application.Common.Validation;
using Iccu.Domain.Common.Enums;

public class DocumentNumberTests
{
    [Theory]
    [InlineData("AA1234567")]
    [InlineData("aa 1234567")]
    [InlineData("AD-1234567")]
    public void IsValid_ForAPassportOrIdCard_AcceptsTwoLettersAndSevenDigits(string documentNumber)
    {
        Assert.True(DocumentNumber.IsValid(DocumentType.Passport, documentNumber));
    }

    [Theory]
    [InlineData("A1234567")]
    [InlineData("AA123456")]
    [InlineData("AA12345678")]
    [InlineData("1234567AA")]
    [InlineData("")]
    public void IsValid_ForAMalformedPassport_Rejects(string documentNumber)
    {
        Assert.False(DocumentNumber.IsValid(DocumentType.Passport, documentNumber));
    }

    [Theory]
    [InlineData("I-TN 1234567")]
    [InlineData("I-ТН № 0123456")]
    [InlineData("123456")]
    public void IsValid_ForABirthCertificate_AcceptsLatinOrCyrillicSeries(string documentNumber)
    {
        Assert.True(DocumentNumber.IsValid(DocumentType.BirthCertificate, documentNumber));
    }

    [Theory]
    [InlineData("ABCDEF")]
    [InlineData("12345")]
    public void IsValid_ForABirthCertificateWithoutDigitsOrTooShort_Rejects(string documentNumber)
    {
        Assert.False(DocumentNumber.IsValid(DocumentType.BirthCertificate, documentNumber));
    }

    [Fact]
    public void Normalize_Should_TreatDifferentlyTypedNumbersAsTheSameDocument()
    {
        Assert.Equal(DocumentNumber.Normalize("aa 123-4567"), DocumentNumber.Normalize("AA1234567"));
        Assert.Equal("ITN1234567", DocumentNumber.Normalize("I-TN № 1234567"));
    }
}
