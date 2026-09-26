namespace Iccu.UnitTests.Common;

using Iccu.Domain.Common;
using Iccu.Application.Common.Validation;

public class PersonDetailsNormalizerTests
{
    [Fact]
    public void Normalized_Should_TrimNamesAndCanonicalizePhoneAndDocument()
    {
        PersonDetails normalized = TestData.Student().Normalized();

        Assert.Equal("Karimov", normalized.LastName);
        Assert.Null(normalized.MiddleName);
        Assert.Equal("+998901234567", normalized.Phone);
        Assert.Equal("AA1234567", normalized.DocumentNumber);
    }

    [Fact]
    public void Normalized_Should_KeepTheApostropheTheVisitorTyped()
    {
        PersonDetails normalized = TestData.Pupil().Normalized();

        Assert.Equal("To‘xtayeva", normalized.LastName);
        Assert.Equal("ITN1234567", normalized.DocumentNumber);
    }
}
