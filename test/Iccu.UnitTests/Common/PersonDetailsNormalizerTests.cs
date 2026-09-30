namespace Iccu.UnitTests.Common;

using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Validation;

public class PersonDetailsNormalizerTests
{
    [Fact]
    public void Normalized_Should_TrimNamesAndCanonicalizePhone()
    {
        PersonDetails normalized = TestData.Student().Normalized();

        Assert.Equal("Karimov", normalized.LastName);
        Assert.Null(normalized.MiddleName);
        Assert.Equal("+998901234567", normalized.Phone);
    }

    [Fact]
    public void Normalized_Should_KeepTheApostropheTheVisitorTyped()
    {
        PersonDetails normalized = TestData.Pupil().Normalized();

        Assert.Equal("To‘xtayeva", normalized.LastName);
    }

    [Fact]
    public void Normalized_ForAForeignCitizen_Should_CanonicalizeTheInternationalPhone()
    {
        PersonDetails normalized = TestData.Student(phone: "+7 (901) 234-56-78", citizenship: Citizenship.Foreign)
            .Normalized();

        Assert.Equal("+79012345678", normalized.Phone);
    }
}
