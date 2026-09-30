namespace Iccu.UnitTests.Common;

using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Validation;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+998 90 123 45 67")]
    [InlineData("998901234567")]
    [InlineData("90 123-45-67")]
    [InlineData("(90) 123 45 67")]
    public void Normalize_Should_ProduceTheInternationalForm(string phone)
    {
        Assert.Equal("+998901234567", PhoneNumber.Normalize(phone, Citizenship.Uzbekistan));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("+7 901 234 56 78")]
    [InlineData("99890123456")]
    [InlineData("")]
    [InlineData(null)]
    public void Normalize_WhenTheNumberIsNotUzbek_ReturnsNull(string? phone)
    {
        Assert.Null(PhoneNumber.Normalize(phone, Citizenship.Uzbekistan));
    }

    [Fact]
    public void Normalize_WithoutACitizenship_AcceptsOnlyUzbekNumbers()
    {
        Assert.Equal("+998901234567", PhoneNumber.Normalize("90 123 45 67", null));
        Assert.Null(PhoneNumber.Normalize("+7 901 234 56 78", null));
    }

    [Theory]
    [InlineData("+7 901 234 56 78", "+79012345678")]
    [InlineData("+49 (151) 1234-5678", "+4915112345678")]
    [InlineData("12345678", "+12345678")]
    [InlineData("90 123 45 67", "+998901234567")]
    [InlineData("+998 90 123 45 67", "+998901234567")]
    public void Normalize_ForAForeignCitizen_AcceptsInternationalNumbers(string phone, string expected)
    {
        Assert.Equal(expected, PhoneNumber.Normalize(phone, Citizenship.Foreign));
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("+1234567890123456")]
    [InlineData("0049 151 1234 5678")]
    [InlineData("")]
    [InlineData(null)]
    public void Normalize_ForAForeignCitizen_RejectsNumbersOutsideTheInternationalRange(string? phone)
    {
        Assert.Null(PhoneNumber.Normalize(phone, Citizenship.Foreign));
    }
}
