namespace Iccu.UnitTests.Common;

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
        Assert.Equal("+998901234567", PhoneNumber.Normalize(phone));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("+7 901 234 56 78")]
    [InlineData("99890123456")]
    [InlineData("")]
    [InlineData(null)]
    public void Normalize_WhenTheNumberIsNotUzbek_ReturnsNull(string? phone)
    {
        Assert.Null(PhoneNumber.Normalize(phone));
    }
}
